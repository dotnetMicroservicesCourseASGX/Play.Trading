using System;
using Automatonymous;
using Play.Identity.Contracts;
using Play.Inventory.Contracts;
using Play.Trading.Service.Activities;
using Play.Trading.Service.Contracts;

namespace Play.Trading.Service.StateMachines;

public class PurchaseStateMachine : MassTransitStateMachine<PurchaseState>
{
    public PurchaseStateMachine()
    {
        // we find where is that we are going to keep tracking the state of the purchase
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureAny();
        COnfigureAccepted();
        ConfigureItemsGranted();
    }

    public State Accepted { get; }
    public State ItemsGranted { get; }
    public State Completed { get; }
    public State Faulted { get; }
    public Event<PurchaseRequested> PurchaseRequested { get; }
    public Event<GetPurchaseState> GetPurchaseState { get; }
    public Event<InventoryItemsGranted> InventoryItemsGranted { get; }
    public Event<GilDebited> GilDebited { get; }

    private void ConfigureEvents()
    {
        Event(() => PurchaseRequested);
        Event(() => GetPurchaseState);
        Event(() => InventoryItemsGranted);
        Event(() => GilDebited);
    }

    private void COnfigureAccepted()
    {
        During(Accepted,
            When(InventoryItemsGranted)
                .Then(context =>
                {
                    context.Instance.LastUpdated = DateTimeOffset.UtcNow;
                })
                .Send(context => new DebitGil(
                    context.Instance.UserId,
                    context.Instance.PurchaseTotal.Value,
                    context.Instance.CorrelationId
                    ))
                .TransitionTo(ItemsGranted)
        );
    }

    private void ConfigureItemsGranted()
    {
        During(ItemsGranted,
            When(GilDebited)
                .Then(context =>
                {
                    context.Instance.LastUpdated = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Completed)
        );
    }

    private void ConfigureAny()
    {
        DuringAny(
            When(GetPurchaseState)
            .Respond(x => x.Instance)
        );
    }

    private void ConfigureInitialState()
    {
        Initially(
            When(PurchaseRequested)
                .Then(context =>
                {
                    context.Instance.UserId = context.Data.UserId;
                    context.Instance.ItemId = context.Data.ItemId;
                    context.Instance.Quantity = context.Data.Quantity;
                    context.Instance.Received = DateTimeOffset.UtcNow;
                    context.Instance.LastUpdated = context.Instance.Received;
                })
                .Activity(x => x.OfType<CalculatePurchaseTotalActivity>())
                .Send(context => new GrantItems(
                    context.Instance.UserId,
                    context.Instance.ItemId,
                    context.Instance.Quantity,
                    context.Instance.CorrelationId))
                .TransitionTo(Accepted)
                .Catch<Exception>(ex => ex.Then(context =>
                {
                    context.Instance.ErrorMessage = context.Exception.Message;
                    context.Instance.LastUpdated = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Faulted))
            );
    }
}

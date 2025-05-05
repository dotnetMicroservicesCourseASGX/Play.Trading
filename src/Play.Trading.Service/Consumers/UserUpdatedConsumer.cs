using System;
using System.Threading.Tasks;
using MassTransit;
using Play.Common;
using Play.Identity.Contracts;
using Play.Trading.Service.Entities;

namespace Play.Trading.Service.Consumers;

public class UserUpdatedConsumer : IConsumer<UserUpdated>
{
    private readonly IRepository<ApplicationUser> _userRepository;

    public UserUpdatedConsumer(IRepository<ApplicationUser> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Consume(ConsumeContext<UserUpdated> context)
    {
        var message = context.Message;

        var user = await _userRepository.GetAsync(message.UserId);
        if (user == null)
        {
            user = new ApplicationUser
            {
                Id = message.UserId,
                Gil = message.NewTotalGil
            };
            await _userRepository.CreateAsync(user);
        }
        else
        {
            user.Gil = message.NewTotalGil;
            await _userRepository.UpdateAsync(user);
        }
    }
}

using HomeYatra.Models;
using HomeYatra.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HomeYatra.Interfaces
{
    public interface IMessageProvider
    {
        Task<ActionResult<MessageResponse>> SendMessageAsync(MessageRequest msgRequest);  
    }
}

using HomeYatra.Models;
using HomeYatra.ViewModels;
using HomeYatra.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace HomeYatra.Services
{
    public class MessageService : IMessageProvider
    {
    
        private readonly string? _WhatsAppFromNumber;
        private readonly MessageDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _messagesTableClient;
        private readonly TableClient _userAccountTableClient;
        private readonly TableClient _otpsTableClient;
        private readonly IWhatsAppService _whatsAppService;

        public MessageService(IConfiguration configuration, IWhatsAppService whatsAppService)
        {
            _configuration = configuration;
            var connectionString = _configuration.GetSection("AzureTableStorage:ConnectionString").Value;
            var messagesTable = _configuration.GetSection("AzureTableStorage:MessagesTable").Value;
            var userAccountTable = _configuration.GetSection("AzureTableStorage:UserAccountTable").Value;
            var otpsTable = _configuration.GetSection("AzureTableStorage:OTPsTable").Value;
            _tableServiceClient = new TableServiceClient(connectionString);
            _messagesTableClient = _tableServiceClient.GetTableClient(messagesTable);
            _userAccountTableClient = _tableServiceClient.GetTableClient(userAccountTable);
            _otpsTableClient = _tableServiceClient.GetTableClient(otpsTable);
          
            _WhatsAppFromNumber = _configuration.GetSection("WhatsApp:FromNumber").Value;
            _whatsAppService = whatsAppService;
        }

        public async Task<MessageResponse> SendMessageAsync(MessageRequest msgRequest)
        {
            var msgResponse = new MessageResponse();
            if (!string.IsNullOrEmpty(msgRequest.Phone))
            {
                //WhatsApp
                
                    var data = await SaveRecords(msgRequest);
                    if (data)
                        msgResponse = await WhatsAppSendMessage(msgRequest);
                    if (msgResponse.StatusCode == 200)
                        msgResponse.Message = "WhatsApp Sent Successfully";
                    return msgResponse;
               
                return msgResponse;
            }

            throw new Exception("Exception Occurred while sending the message");
        }


        #region

        private string GenerateOtp()
        {
            Random random = new Random();
            int otp = random.Next(100000, 999999); // Generates a 6-digit number
            return otp.ToString();
        }



        private async Task<bool> SaveRecords(MessageRequest msgRequest)
        {
            var whatsAppUsualTemplates = new List<int?> { 0, 1, 2 };
            try
            {
                
                //WhatsApp
                if (msgRequest.Phone.Count() > 0 && msgRequest.ChannelId == 3 && whatsAppUsualTemplates.Contains(msgRequest.TemplateId))
                {
                    foreach (var contact in msgRequest.Contacts)
                    {
                        var partitionKey = "Partition1";
                        var rowKey = Guid.NewGuid().ToString();
                        var tableEntity = new Messages()
                        {
                            PartitionKey = partitionKey,
                            RowKey = rowKey,
                            UserId = msgRequest.UserId,
                            FromNumber = _WhatsAppFromNumber,
                            ToNumber = contact.Number,
                            ChannelId = 3,
                            TemplateId = msgRequest.TemplateId,
                            ToContact = contact.Name,
                            Body = msgRequest.Message,
                            ChannelName = "WhatsApp",
                            CreatedDT = DateTimeOffset.UtcNow,
                            Status = "Saved",
                        };

                        var response = await _messagesTableClient.AddEntityAsync(tableEntity);
                    }
                }
                
                //OTP on whatsapp
                if (msgRequest.Phone.Count() == 1 && msgRequest.ChannelId == 3 && msgRequest.TemplateId == 3)
                {
                    var otp = GenerateOtp();
                    msgRequest.OTP = otp;
                    try
                    {
                        var partitionKey = "Partition1";
                            var rowKey = Guid.NewGuid().ToString();
                            var tableEntity = new OTPs()
                            {
                                PartitionKey = partitionKey,
                                RowKey = rowKey,
                                FromNumber = _WhatsAppFromNumber,
                                ToNumber = msgRequest.Phone,
                                OTP = msgRequest.OTP,
                                ChannelName = "WhatsApp",
                                ChannelId = 1,
                                CreatedDT = DateTimeOffset.UtcNow,
                                Status = "Saved",
                            };

                            var response = await _otpsTableClient.AddEntityAsync(tableEntity);
                        
                    }
                    catch (Exception ex)
                    {
                        throw new BadHttpRequestException("Issue occurred with OTP");
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException("Failed to Save Records");
            }
        }



        private async Task<MessageResponse> WhatsAppSendMessage(MessageRequest msgRequest)
        {
            var msgResponse = new MessageResponse();
            try
            {
                if (msgRequest.Phone.Count() > 0 && msgRequest.TemplateId > 0)
                {
                    msgResponse = await SendWhatsAppUsingTemplate(msgRequest);
                }
                if (msgRequest.Phone.Count() > 0 && msgRequest.TemplateId == 0)
                {
                    msgResponse = await SendWhatsAppDirectMessage(msgRequest);
                }
                return msgResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Occured in Sending WhatsApp");
            }
        }

        private async Task<MessageResponse> SendWhatsAppUsingTemplate(MessageRequest msgRequest)
        {
            var parameters = new List<Parameter>();
            var components = new List<Component>();
            var msgResponse = new MessageResponse();
            try
            {
                
                var message = new SendMessageTemplate();
                message.MessagingProduct = "whatsapp";
                message.Type = "template";
                
                //OTP

                if (msgRequest.TemplateId == 3 && msgRequest.Phone != null)
                {
                    message.recipient_type = "individual";

                    var componentWithBodyParams = new Component
                    {
                        type = "body",
                        parameters = new List<Parameter>()
                        {
                            new Parameter
                            {
                                type = "text",
                                text= msgRequest.OTP
                            }
                        }
                    };

                    var componentWithButtonParams = new Component
                    {
                        type = "Button",
                        sub_type = "url",
                        index = "0",
                        parameters = new List<Parameter>()
                        {
                            new Parameter
                            {
                                type = "text",
                                text= msgRequest.OTP
                            }
                        }
                    };

                    message.Template = new Template
                    {
                        Name = "otp",
                        Language = new Language { Code = "en_US" },
                        Components = new List<Component>()
                        {
                            componentWithBodyParams,
                            componentWithButtonParams
                        }
                    };

                }

                //common code for sending messages
                message.To = msgRequest.Phone;
               msgResponse = await _whatsAppService.SendMessageUsingTemplate(message);
                return msgResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save whatsapp messages");
            }
        }

        private async Task<MessageResponse> SendWhatsAppDirectMessage(MessageRequest msgRequest)
        {
            var msgResponse = new MessageResponse();
            try
            {
                var message = new SendTextMessage();
                message.MessagingProduct = "whatsapp";
                message.Type = "text";
                message.RecipientType = "individual";
                message.Text.Body = msgRequest.Message;
                message.To = msgRequest.Phone;
                msgResponse = await _whatsAppService.SendDirectTextMessage(message);
                return msgResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to Send Direct whatsapp messages");
            }
        }
#endregion
    }
}

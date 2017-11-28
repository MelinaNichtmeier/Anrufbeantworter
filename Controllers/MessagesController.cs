using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System.Net.Http;
using System.Web.Http.Description;
using System.Diagnostics;

namespace Microsoft.Bot.Sample.FormBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        internal static IDialog<SandwichOrder> MakeRootDialog()
        {
            //return Chain.From(() => FormDialog.FromForm(SandwichOrder.BuildForm))
            return Chain.From(() => FormDialog.FromForm(SandwichOrder.BuildLocalizedForm))
            .Do(async (context, order) =>
            {
                try
                {
                    var completed = await order;
                    // Actually process the sandwich order...
                    await context.PostAsync("Processed your order!");
                    /*
                        var msg = new MailMessage();
                        msg.From = new MailAddress(“info@YourWebSiteDomain.com”); 
                        msg.To.Add(strTo); 
                        msg.Subject = strSubject; 
                        msg.IsBodyHtml = true; 
                        msg.Body = strMessage;
                        
                        // configure the smtp server
                        var smtp = new SmtpClient(“YourSMTPServer”); 
                        var = new System.Net.NetworkCredential(“YourSMTPServerUserName”, “YourSMTPServerPassword”);
                        
                        // send the message
                        smtp.Send(msg); 
                    */
                }
                catch (FormCanceledException<SandwichOrder> e)
                {
                    string reply;
                    if (e.InnerException == null)
                    {
                        reply = $"You quit on {e.Last} -- maybe you can finish next time!";
                    }
                    else
                    {
                        reply = "Sorry, I've had a short circuit. Please try again.";
                    }
                    await context.PostAsync(reply);
                }
            });
        }
        
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity != null)
            {
                // one of these will have an interface and process it
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, MakeRootDialog);
                        break;

                    case ActivityTypes.ConversationUpdate:
                    /*    IConversationUpdateActivity update = activity;
                        using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                        {
                            var client = scope.Resolve<IConnectorClient>();
                            if (update.MembersAdded.Any())
                            {
                                var reply = activity.CreateReply();
                                var newMembers = update.MembersAdded?.Where(t => t.Id != activity.Recipient.Id);
                                foreach (var newMember in newMembers)
                                {
                                    reply.Text = "Welcome";
                                    if (!string.IsNullOrEmpty(newMember.Name))
                                    {
                                        reply.Text += $" {newMember.Name}";
                                    }
                                    reply.Text += "!";
                                    await client.Conversations.ReplyToActivityAsync(reply);
                                }
                            }
                        }
                        break;
                        */
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }
}
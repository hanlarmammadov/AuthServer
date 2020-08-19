
namespace AuthServer.Mailing.Sender.Models
{
    public class MailModel  
    {
        public string Template { get; private set; }
        public string TemplateFile { get; private set; }
        public string From { get; private set; }
        public string To { get; private set; }
        public string Subject { get; private set; }
        public ViewModel ViewModel { get; private set; }

        public MailModel()
        {

        }

        public MailModel AddTemplate(string template)
        {
            Template = template;
            TemplateFile = null;
            return this;
        }

        public MailModel AddTemplateFile(string path)
        {
            TemplateFile = path;
            Template = null;
            return this;
        }

        public MailModel AddFrom(string from)
        {
            From = from;
            return this;
        }
        public MailModel AddTo(string to)
        {
            To = to;
            return this;
        }
        public MailModel AddSubject(string subject)
        {
            Subject = subject;
            return this;
        }
        public MailModel AddViewModel(ViewModel viewModel)
        {
            ViewModel = viewModel; 
            return this;
        }
    }
}

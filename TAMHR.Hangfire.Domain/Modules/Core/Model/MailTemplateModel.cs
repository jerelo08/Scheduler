using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAMHR.Hangfire.Domain.Modules.Core.Model
{
    public class MailTemplateModel
    {
        public string MailKey { get; set; }
        public string MailFrom { get; set; }
        public string Subject { get; set; }
        public string MailContent { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAMHR.Hangfire.Domain.Modules.Core.Model
{
    public class MailQueueModel
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public string Application { get; set; }
        public string DocumentNumber { get; set; }
        public string Title { get; set; }
        public string CurrentApprover { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string RedirectUrl { get; set; }
        public string FormKey { get; set; }
        public string FormTitle { get; set; }
        public string SubmitterName { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

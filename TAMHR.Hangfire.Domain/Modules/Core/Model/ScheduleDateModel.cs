using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAMHR.Hangfire.Domain.Modules.Core.Model
{
    public class ScheduleDateModel
    {
        [Key]
        public Guid Id { get; set; }
        public string ScheduleName { get; set; }
        public string ConfigTime { get; set; }
        public string ConfigDays { get; set; }
        public string ConfigApps { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int RowStatus { get; set; }
        public string? CreatedBy { get; set; }
    }
}

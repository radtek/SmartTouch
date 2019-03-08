using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class RoundRobinContactAssignmentDb
    {
        [Key]
        public int RoundRobinContactAssignmentID { get; set; }
        public byte DayOfWeek { get; set; }
        public string UserID { get; set; }
        public bool IsRoundRobinAssignment { get; set; }

        [NotMapped]
        public IEnumerable<string> UserNames { get; set; }
        public int WorkFlowUserAssignmentActionID { get; set; }

        public void Save(int workFlowUserAssignmentActionID)
        {
            using (var db = new CRMDb())
            {
                if (this.RoundRobinContactAssignmentID == 0)
                {
                    var assignment = GetObject();
                    assignment.WorkFlowUserAssignmentActionID = workFlowUserAssignmentActionID;

                    db.RoundRobinAssignment.Add(assignment);
                    db.SaveChanges();
                }
                else
                {
                    var assignment = GetObject();
                    if(workFlowUserAssignmentActionID !=0)
                    {
                        assignment.WorkFlowUserAssignmentActionID = workFlowUserAssignmentActionID;

                        db.Entry(assignment).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }

        private RoundRobinContactAssignmentDb GetObject()
        {
            var assignment = new RoundRobinContactAssignmentDb()
            {
                DayOfWeek = this.DayOfWeek,
                IsRoundRobinAssignment = this.IsRoundRobinAssignment,
                RoundRobinContactAssignmentID = this.RoundRobinContactAssignmentID,
                UserID = this.UserID,
                WorkFlowUserAssignmentActionID = this.WorkFlowUserAssignmentActionID
            };
            return assignment;
        }
    }
}

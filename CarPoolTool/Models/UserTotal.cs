using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPoolTool.Models
{
    public class UserTotal
    {
        public UserTotal() { }
        public UserTotal(User user, Total total)
        {
            Total = total;
            User = user;
        }
        public User User { get; set; }
        public Total Total { get; set; }

        public static UserTotal Get(User user)
        {
            CarPoolToolEntities entities = new CarPoolToolEntities();

            var usertotals = from b in entities.Totals
                             where b.username == user.username
                             select b;
            Total total = usertotals.FirstOrDefault();
            
            return new UserTotal(user, total);
        }

    }
}
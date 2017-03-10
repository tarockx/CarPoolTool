using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPoolTool.Models
{
    public partial class Alert
    {
        public bool isValid()
        {
            return (
                !string.IsNullOrEmpty(username) &&
                !string.IsNullOrEmpty(message) &&
                !string.IsNullOrEmpty(severity) &&
                data != null
                );
        }
    }
}
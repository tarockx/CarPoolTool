﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CarPoolTool.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CarPoolToolEntities : DbContext
    {
        public CarPoolToolEntities()
            : base("name=CarPoolToolEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CarpoolLog> CarpoolLogs { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Total> Totals { get; set; }
    }
}
using act.core.data.Extensibility;
using Microsoft.EntityFrameworkCore;

namespace act.core.data
{
    public class ActDbContext : DbContext
    {
        public ActDbContext(DbContextOptions<ActDbContext> options) : base(options)
        {
        }
        public virtual DbSet<BuildSpecification> BuildSpecifications { get; set; }
        public virtual DbSet<ComplianceResult> ComplianceResults { get; set; }
        public virtual DbSet<ComplianceResultError> ComplianceResultErrors { get; set; }
        public virtual DbSet<ComplianceResultTest> ComplianceResultTests { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Function> Functions { get; set; }
        public virtual DbSet<Environment> Environments { get; set; }
        public virtual DbSet<Justification> Justifications { get; set; }
        public virtual DbSet<Node> Nodes { get; set; }
        public virtual DbSet<Port> Ports { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<SoftwareComponent> SoftwareComponents { get; set; }
        public virtual DbSet<SoftwareComponentEnvironment> SoftwareComponentEnvironments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         
            modelBuilder.ModelPluralizer<BuildSpecification>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .IsUnique();

                entity.HasIndex(e => e.OwnerEmployeeId);

                entity.HasIndex(e => e.ParentId);

                entity.Property(e => e.EmailAddress).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.OperatingSystemName).HasMaxLength(256);

                entity.Property(e => e.OperatingSystemVersion).HasMaxLength(32);

                entity.Property(e => e.OwnerEmployeeId);

                entity.Property(e => e.TimeStamp)
                    .IsRequired()
                    .IsRowVersion();

                entity.Property(e => e.WikiLink).HasMaxLength(256);

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.BuildSpecifications)
                    .HasForeignKey(d => d.OwnerEmployeeId);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Children)
                    .HasForeignKey(d => d.ParentId);
            });

            modelBuilder.ModelPluralizer<ComplianceResult>(entity =>
            {
                entity.HasIndex(e => new {e.EndDate, e.Id, e.InventoryItemId});

                entity.HasIndex(e => new {e.ResultId, e.InventoryItemId})
                    .IsUnique();

                entity.Property(e => e.EndDate)
                    .HasColumnType("date");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.ComplianceResults)
                    .HasForeignKey(d => d.InventoryItemId);
            });

            modelBuilder.ModelPluralizer<ComplianceResultError>(entity =>
            {
                entity.HasIndex(e => new
                {
                    e.ComplianceResultId,
                    e.Code,
                    e.Name
                });
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.LongMessage).IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.ComplianceResult)
                    .WithMany(p => p.Errors)
                    .HasForeignKey(d => d.ComplianceResultId);
            });

            modelBuilder.ModelPluralizer<ComplianceResultTest>(entity =>
            {
                entity.HasIndex(e => new
                {
                    e.ComplianceResultId,
                    e.ResultType,
                    e.PortType,
                    e.ShouldExist,
                    e.Name
                });

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.ComplianceResult)
                    .WithMany(p => p.Tests)
                    .HasForeignKey(d => d.ComplianceResultId);
            });

            modelBuilder.ModelPluralizer<Employee>(entity =>
            {

                entity.HasIndex(e => e.SamAccountName)
                    .IsUnique();
                
                entity.HasIndex(e => new {e.FirstName, e.Id});

                entity.HasIndex(e => new {e.LastName, e.Id});

                entity.HasIndex(e => new {e.PreferredName, e.Id});

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(e => e.PreferredName).HasMaxLength(64);

                entity.Property(e => e.SamAccountName)
                    .IsRequired()
                    .HasColumnType("nchar(64)");
                
                entity.HasOne(p=>p.ReportingDirector)
                    .WithMany()
                    .HasForeignKey(d => d.ReportingDirectorId);

                entity.HasOne(d => d.Supervisor)
                    .WithMany()
                    .HasForeignKey(d => d.SupervisorId);
            });

            modelBuilder.ModelPluralizer<Function>(entity =>
            {
                entity.HasIndex(e => e.Name);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.ModelPluralizer<SoftwareComponentEnvironment>(entity =>
            {
                entity.HasKey(p => new {p.EnvironmentId, p.SoftwareComponentId});

                entity.HasOne(e => e.Environment)
                    .WithMany(s => s.SoftwareComponentEnvironments)
                    .HasForeignKey(p => p.EnvironmentId);

                entity.HasOne(e => e.SoftwareComponent)
                    .WithMany(s => s.SoftwareComponentEnvironments)
                    .HasForeignKey(p => p.SoftwareComponentId);

            });
            
            modelBuilder.ModelPluralizer<Environment>(entity =>
            {
                
                entity.Property(e => e.Id).ValueGeneratedNever();
                
                entity.HasIndex(e => e.Name)
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(32);
                
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.ChefAutomateUrl)
                    .IsRequired()
                    .HasMaxLength(256);
                
                entity.Property(e => e.ChefAutomateOrg)
                    .IsRequired()
                    .HasMaxLength(16);
                
                entity.Property(e => e.ChefAutomateToken)
                    .IsRequired()
                    .HasMaxLength(64);
                
                entity.Property(e => e.Color)
                    .IsRequired()
                    .HasMaxLength(7);
                
                entity.HasMany(d => d.SoftwareComponentEnvironments)
                    .WithOne(p => p.Environment)
                    .HasForeignKey(d => d.EnvironmentId);
            });

            modelBuilder.ModelPluralizer<Justification>(entity =>
            {
                entity.HasIndex(e => e.BuildSpecificationId);

                entity.Property(e => e.Color);

                entity.Property(e => e.JustificationText).IsRequired();

                entity.Property(e => e.TimeStamp)
                    .IsRequired()
                    .IsRowVersion();

                entity.HasOne(d => d.BuildSpecification)
                    .WithMany(p => p.Justifications)
                    .HasForeignKey(d => d.BuildSpecificationId);
            });

            modelBuilder.ModelPluralizer<Node>(entity =>
            {
                entity.HasKey(e => e.InventoryItemId);

                entity.HasIndex(e => e.BuildSpecificationId);

                entity.HasIndex(e => e.FunctionId);

                entity.HasIndex(e => e.Platform);
                
                entity.HasIndex(e => e.EnvironmentId);

                entity.HasIndex(e => e.OwnerEmployeeId);

                entity.HasIndex(e => e.ProductCode);

                entity.HasIndex(e => new {e.Fqdn, e.InventoryItemId});

                entity.HasIndex(e => new {e.PciScope, e.InventoryItemId});

                entity.Property(e => e.InventoryItemId).ValueGeneratedNever();

                entity.Property(e => e.ComplianceStatus);

                entity.Property(e => e.DeactivatedDate).HasColumnType("datetime");

                entity.Property(e => e.Platform);

                entity.Property(e => e.Fqdn).HasMaxLength(256);

                entity.Property(e => e.LastComplianceResultDate).HasColumnType("datetime");
                
                entity.Property(e => e.FailingSince).HasColumnType("datetime");
                
                entity.Property(e => e.LastEmailedOn).HasColumnType("datetime");

                entity.Property(e => e.PciScope).HasDefaultValueSql("4");

                entity.Property(e => e.ProductCode).HasColumnType("nchar(4)");

                entity.HasOne(d => d.BuildSpecification)
                    .WithMany(p => p.Nodes)
                    .HasForeignKey(d => d.BuildSpecificationId);

                entity.HasOne(d => d.Function)
                    .WithMany(p => p.Nodes)
                    .HasForeignKey(d => d.FunctionId);

                entity.HasOne(d => d.Environment)
                    .WithMany(p => p.Nodes)
                    .HasForeignKey(d => d.EnvironmentId);

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Nodes)
                    .HasForeignKey(d => d.OwnerEmployeeId);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Nodes)
                    .HasForeignKey(d => d.ProductCode);
            });

            modelBuilder.ModelPluralizer<Port>(entity =>
            {
                entity.HasIndex(e => e.BuildSpecificationId);

                entity.HasIndex(e => e.JustificationId);

                entity.Property(e => e.TimeStamp)
                    .IsRequired()
                    .IsRowVersion();

                entity.HasOne(d => d.BuildSpecification)
                    .WithMany(p => p.Ports)
                    .HasForeignKey(d => d.BuildSpecificationId);

                entity.HasOne(d => d.Justification)
                    .WithMany(p => p.Ports)
                    .HasForeignKey(d => d.JustificationId);
            });

            modelBuilder.ModelPluralizer<Product>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.HasIndex(e => e.Name);

                entity.Property(e => e.Code)
                    .HasColumnType("nchar(4)")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.ModelPluralizer<SoftwareComponent>(entity =>
            {
                entity.HasIndex(e => e.BuildSpecificationId);

                entity.HasIndex(e => e.JustificationId);

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.TimeStamp)
                    .IsRequired()
                    .IsRowVersion();

                entity.HasMany(d => d.SoftwareComponentEnvironments)
                    .WithOne(p => p.SoftwareComponent)
                    .HasForeignKey(d => d.SoftwareComponentId);
              
                entity.HasOne(d => d.BuildSpecification)
                    .WithMany(p => p.SoftwareComponents)
                    .HasForeignKey(d => d.BuildSpecificationId);

                entity.HasOne(d => d.Justification)
                    .WithMany(p => p.SoftwareComponents)
                    .HasForeignKey(d => d.JustificationId);
            });
        }
    }
}
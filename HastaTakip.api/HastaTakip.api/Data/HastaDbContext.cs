    using HastaTakip.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Reflection.Emit;

    namespace HastaTakip.Api.Data
    {
        public class HastaDbContext : DbContext
        {
            public HastaDbContext(DbContextOptions<HastaDbContext> options)
                : base(options) { }

            // DbSet'ler
            public DbSet<Patient> Hastalar => Set<Patient>();
            public DbSet<Ziyaret> Ziyaretler => Set<Ziyaret>();
            public DbSet<Antropometri> Antropometriler => Set<Antropometri>();
            public DbSet<AileUyesi> AileUyeleri => Set<AileUyesi>();
            public DbSet<Diyet> Diyetler => Set<Diyet>();
            public DbSet<Oykuler> Oykuler => Set<Oykuler>();
            public DbSet<PuberteFizik> PuberteFizikler => Set<PuberteFizik>();
            public DbSet<YorumPlan> YorumPlanlar => Set<YorumPlan>();
            public DbSet<OzetHesapKlinik> OzetHesaplar => Set<OzetHesapKlinik>();
            public DbSet<GrowthLMS> GrowthLMS { get; set; } = default!;
            public DbSet<LabSonuc> LabSonuclari { get; set; }
            public DbSet<LabParametre> LabParametreler { get; set; }
            public DbSet<User> Users => Set<User>();



        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<User>(e =>
            {
                e.ToTable("Users");

                e.HasKey(x => x.UserID);

                e.Property(x => x.Username)
                 .HasMaxLength(50)
                 .IsRequired();

                e.Property(x => x.Password)
                 .HasMaxLength(100)
                 .IsRequired();

                e.Property(x => x.Role)
                 .HasMaxLength(20)
                 .IsRequired();

                e.HasIndex(x => x.Username)
                 .IsUnique();
            });

            // klinik.Hasta

            b.Entity<Patient>(e =>
            {
                e.ToTable("Hasta", "klinik", tb =>
                {
                    
                    tb.HasTrigger("trg_Hasta_AfterUpdate_DogumTarihi");
                });

                e.HasKey(p => p.Id);
                e.Property(p => p.Id).HasColumnName("HastaID");
                e.Property(p => p.TcKimlikNo).HasMaxLength(11).IsRequired();
                e.Property(p => p.Ad).HasMaxLength(50).IsRequired();
                e.Property(p => p.Soyad).HasMaxLength(50).IsRequired();
                e.Property(p => p.BirthDate).HasColumnName("DogumTarihi").HasColumnType("date").IsRequired();
                e.Property(p => p.Cinsiyet).HasMaxLength(1).IsRequired();
                e.Property(p => p.Telefon).HasMaxLength(20);
                e.Property(p => p.Email).HasMaxLength(100);
                e.Property(p => p.Adres).HasMaxLength(250);
                e.Property(p => p.KayitTarihi).HasColumnType("datetime2(7)").IsRequired();
            });

            
            //  klinik.Ziyaret
            
            b.Entity<Ziyaret>(e =>
            {
                e.ToTable("Ziyaret", "klinik");
                e.HasKey(z => z.ZiyaretID);

                e.Property(z => z.Tarih).HasColumnType("datetime2(7)").IsRequired();
                e.Property(z => z.Notlar).HasMaxLength(500);
                e.Property(z => z.YakinmalarZiyaret).HasColumnType("nvarchar(max)");

                e.HasOne(z => z.Hasta)
                 .WithMany()
                 .HasForeignKey(z => z.HastaID)
                 .HasPrincipalKey(p => p.Id);

                e.HasIndex(z => new { z.HastaID, z.Tarih })
                 .HasDatabaseName("IX_Ziyaret_Hasta_Tarih");
            });

            
            //  klinik.Antropometri
            
            b.Entity<Antropometri>(e =>
            {
                e.ToTable("Antropometri", "klinik", tb =>
                {
                    
                    tb.HasTrigger("trg_Antropometri_AfterIU");
                });

                e.HasKey(a => a.AntropometriID);

                e.HasOne(a => a.Ziyaret)
                 .WithMany(z => z.Antropometriler)
                 .HasForeignKey(a => a.ZiyaretID)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(a => a.YasAy).HasColumnType("int");

                e.Property(a => a.BoyCm).HasColumnType("decimal(5,2)");
                e.Property(a => a.KiloKg).HasColumnType("decimal(5,2)");
                e.Property(a => a.BasCevresiCm).HasColumnType("decimal(5,2)");
                e.Property(a => a.OturmaBoyuCm).HasColumnType("decimal(5,2)");
                e.Property(a => a.ObTb).HasColumnType("decimal(5,2)");
                e.Property(a => a.GogusCevresiCm).HasColumnType("decimal(5,2)");
                e.Property(a => a.BasPubisCm).HasColumnType("decimal(5,2)");
                e.Property(a => a.PubisTopukCm).HasColumnType("decimal(5,2)");
                e.Property(a => a.BoySDS).HasColumnType("decimal(5,2)");
                e.Property(a => a.KiloSDS).HasColumnType("decimal(5,2)");
                e.Property(a => a.BKISDS).HasColumnType("decimal(5,2)");
                e.Property(a => a.BasCevresiSDS).HasColumnType("decimal(5,2)");
                e.Property(a => a.YBHSDS).HasColumnType("decimal(5,2)");

                e.Property(a => a.BKI)
                 .HasColumnType("decimal(18,4)")
                 .ValueGeneratedOnAddOrUpdate();

                e.HasIndex(a => a.ZiyaretID).HasDatabaseName("IX_Antropometri_Ziyaret");
            });

            
            // klinik.LabParametre
            
            b.Entity<LabParametre>(e =>
            {
                e.ToTable("LabParametre", "klinik");
                e.HasIndex(x => x.Kod).IsUnique();
            });

            
            // klinik.LabSonuc
            
            b.Entity<LabSonuc>(e =>
            {
                e.ToTable("LabSonuc", "klinik");
                e.HasKey(x => x.LabSonucID);

                e.Property(x => x.Deger).HasMaxLength(50);
                e.Property(x => x.DegerSayisal).HasColumnType("decimal(10,3)");
                e.Property(x => x.RefAlt).HasColumnType("decimal(10,3)");
                e.Property(x => x.RefUst).HasColumnType("decimal(10,3)");

                e.HasIndex(x => new { x.ZiyaretID, x.Tarih });
            });


           
            //  klinik.AileUyesi
           
            b.Entity<AileUyesi>(e =>
            {
                e.ToTable("AileUyesi", "klinik");
                e.HasOne(x => x.Hasta)
                 .WithMany()
                 .HasForeignKey(x => x.HastaID)
                 .HasPrincipalKey(p => p.Id)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.HastaID).HasDatabaseName("IX_AileUyesi_Hasta");
                e.Property(x => x.Iliski).HasMaxLength(20).IsRequired();
                e.Property(x => x.Ad).HasMaxLength(50);
                e.Property(x => x.SaglikDurumu).HasMaxLength(100);
                e.Property(x => x.Meslek).HasMaxLength(100);

                e.Property(x => x.BoyCm).HasColumnType("decimal(5,2)");
                e.Property(x => x.AgirlikKg).HasColumnType("decimal(5,2)");
                e.Property(x => x.PuberteYasiYil).HasColumnType("decimal(4,2)");
            });

           
            // klinik.Diyet
            
            b.Entity<Diyet>(e =>
            {
                e.ToTable("Diyet", "klinik");

                e.HasOne(x => x.Hasta)
                 .WithMany()
                 .HasForeignKey(x => x.HastaID)
                 .HasPrincipalKey(p => p.Id)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.HastaID, x.Tarih })
                 .HasDatabaseName("IX_Diyet_Hasta_Tarih");

                e.Property(x => x.Tarih)
                 .HasColumnType("date");

                e.Property(x => x.Ekmek).HasMaxLength(50);
                e.Property(x => x.Tahil).HasMaxLength(50);

                e.Property(x => x.Et).HasMaxLength(50);
                e.Property(x => x.Peynir).HasMaxLength(50);

                e.Property(x => x.Sut).HasMaxLength(50);
                e.Property(x => x.Yogurt).HasMaxLength(50);

                e.Property(x => x.Meyve).HasMaxLength(50);
                e.Property(x => x.Sebze).HasMaxLength(50);

                e.Property(x => x.SiviGida).HasMaxLength(50);
                e.Property(x => x.AburCubur).HasMaxLength(50);
                e.Property(x => x.EkranSuresi).HasMaxLength(50);
            });

            
            //  klinik.Oykuler
            
            b.Entity<Oykuler>(e =>
            {
                e.ToTable("Oykuler", "klinik");

                e.HasOne(x => x.Hasta)
                 .WithMany()
                 .HasForeignKey(x => x.HastaID)
                 .HasPrincipalKey(p => p.Id)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.HastaID)
                 .HasDatabaseName("IX_Oykuler_Hasta");

                e.Property(x => x.DogumSekli)
                 .HasMaxLength(20);

                
                e.Property(x => x.DogumAgirlik)
                 .HasMaxLength(50);

                e.Property(x => x.DogumBoyu)
                 .HasMaxLength(50);

                e.Property(x => x.OlusturmaTarihi)
                 .HasColumnType("datetime2(7)");
            });

            
            //  klinik.PuberteFizik
            
            b.Entity<PuberteFizik>(e =>
            {
                e.ToTable("PuberteFizik", "klinik");

                e.HasOne(x => x.Ziyaret)
                 .WithMany(z => z.PuberteFizikler)
                 .HasForeignKey(x => x.ZiyaretID)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.ZiyaretID).HasDatabaseName("IX_PuberteFizik_Ziyaret");
            });

            
            //  klinik.YorumPlan
            
            b.Entity<YorumPlan>(e =>
            {
                e.ToTable("YorumPlan", "klinik");

                e.HasOne(x => x.Ziyaret)
                 .WithMany(z => z.YorumPlanlari)
                 .HasForeignKey(x => x.ZiyaretID)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(x => x.OlusturmaTarihi).HasColumnType("datetime2(7)");
                e.HasIndex(x => new { x.ZiyaretID, x.OlusturmaTarihi }).HasDatabaseName("IX_YorumPlan_Ziyaret_Tarih");
            });

            
            //  klinik.OzetHesaplar
            
            b.Entity<OzetHesapKlinik>(e =>
            {
                e.ToTable("OzetHesaplar", "klinik");
                e.HasKey(x => x.OzetID);

                e.Property(x => x.KulacCm).HasColumnType("decimal(5,2)");
                e.Property(x => x.HedefBoyCm).HasColumnType("decimal(5,2)");
                e.Property(x => x.BoyaUyanTarti).HasMaxLength(50);

                e.HasOne(x => x.Ziyaret)
                 .WithMany(z => z.OzetHesaplar)
                 .HasForeignKey(x => x.ZiyaretID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            
            // klinik.GrowthLMS
            
            b.Entity<GrowthLMS>(e =>
            {
                e.ToTable("GrowthLMS", "klinik");
                e.HasKey(x => x.GrowthID);

                e.Property(x => x.Kaynak).HasMaxLength(10).IsRequired();
                e.Property(x => x.Olcum).HasMaxLength(200).IsRequired();
                e.Property(x => x.Cinsiyet).HasMaxLength(1).IsRequired();

                e.Property(x => x.YasAy).IsRequired();
                e.Property(x => x.L).IsRequired();
                e.Property(x => x.M).IsRequired();
                e.Property(x => x.S).IsRequired();

                e.HasIndex(x => new { x.Kaynak, x.Olcum, x.Cinsiyet, x.YasAy }).IsUnique();
            });

        }

    }
}

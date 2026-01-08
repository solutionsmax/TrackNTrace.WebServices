using Microsoft.EntityFrameworkCore;
using TrackNTrace.Repository.Common;
using TrackNTrace.Repository.Interfaces;
using TrackNTrace.Repository.Utilities;
using TrackNTrace.WebServices.com.AgencyValidation;
using TrackNTrace.WebServices.com.Interfaces;

var builder = WebApplication.CreateBuilder(args);


//// Add services to the container.
builder.Services.AddDbContext<TrackNTrace.Repository.Entities.TrackNTraceContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("ClinetTrackNTraceDB")));
builder.Services.AddDbContext<TrackNTrace.Repository.StoredProcedures.TNTSPsContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("ClinetTrackNTraceDB")));
builder.Services.AddTransient <IBatchHelper, BatchHelper>();
builder.Services.AddTransient<IPackagingLinesSetupHelper, PackagingLinesSetupHelper>();
builder.Services.AddTransient<IPriorScanValidationHelper, PriorScanValidationHelper>();
builder.Services.AddTransient<IGtinHelper, GtinHelper>();
builder.Services.AddTransient<IValidationRuleHelper, ValidationRuleHelper>();
builder.Services.AddTransient<IPackageLebelingHelper, PackageLebelingHelper>();
builder.Services.AddTransient<IPackagingPackSplit, PackagingPackSplit>();
builder.Services.AddTransient<ISerializationHelper, SerializationHelper>();
builder.Services.AddTransient<IFncCharHelper, FncCharHelper>();
builder.Services.AddTransient<IPackageConfigurationLevel, PackageConfigurationLevel>();
builder.Services.AddTransient<IAnvisa55_7, Anvisa55_7>();
builder.Services.AddTransient<IBarcodeProfiler, BarcodeProfilerHelper>();
builder.Services.AddTransient<ICommonAgency, CommonAgency>();
builder.Services.AddTransient<INMPAChina86, NMPAChina86>();
builder.Services.AddTransient<ISharedBLL, SharedBLL>();
builder.Services.AddTransient<ISingeCottnPack, SingeCottnPack>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
 
 

app.UseAuthorization();

app.MapControllers();

app.Run();

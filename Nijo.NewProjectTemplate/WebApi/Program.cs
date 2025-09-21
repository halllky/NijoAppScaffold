var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// swagger。デバッグのためにこのアプリで定義されているエンドポイントを一覧する
builder.Services.AddSwaggerGen();

// NijoApplicationBuilderの設定
var appConfig = new MyApp.OverridedApplicationConfigure();
appConfig.ConfigureServices(builder.Services);
builder.Services.AddScoped<MyApp.DefaultConfigurationInWebApi, MyApp.WebApi.Base.ConfigurationInWebApi>();

// JSONシリアライズ設定
builder.Services.ConfigureHttpJsonOptions(options => {
    appConfig.EditDefaultJsonSerializerOptions(options.SerializerOptions);
});

// --------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();

    // swagger。デバッグのためにこのアプリで定義されているエンドポイントを一覧する
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary) {
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

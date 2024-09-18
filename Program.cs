namespace HomeWork1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()    // Дозволяє запити з будь-якого джерела
                           .AllowAnyMethod()    // Дозволяє всі HTTP-методи (GET, POST тощо)
                           .AllowAnyHeader();   // Дозволяє всі заголовки
                });
            });
            var app = builder.Build();
            app.UseCors("AllowAll");
            app.UseStaticFiles();
            int number = 1;
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path;
                DateTime dateBefore = DateTime.Now;
                Console.WriteLine($"{number}) Current date and time before terminal middleware: {dateBefore.ToShortDateString()} {dateBefore.ToLongTimeString()} \nPath={path}");
                number++;
                await next();
                DateTime dateAfter = DateTime.Now;
                Console.WriteLine($"Current date and time after terminal middleware: {dateAfter.ToShortDateString()} {dateAfter.ToLongTimeString()}");
                TimeSpan deltaDate = dateAfter - dateBefore;
                Console.WriteLine($"\u001b[32mThe terminal middleware took {deltaDate.TotalMilliseconds} milliseconds to execute\n\u001b[0m");
            });
            app.Run(async (context) =>
            {
                var request = context.Request;
                var response = context.Response;
                var path = request.Path;

                response.Headers.ContentType = "text/html; charset=utf-8";
                response.StatusCode = 200;

                if (path == "/")
                {
                    if (request.Query.ContainsKey("Name") && request.Query.ContainsKey("Dob") && request.Query.ContainsKey("Gender"))
                    {
                        var name = request.Query["Name"];
                        DateTime dob;

                        if (DateTime.TryParse(request.Query["Dob"], out dob))
                        {
                            // Перетворення успішне, dob тепер є DateOnly
                        }
                        else
                        {
                            dob = new DateTime(1900, 01, 01);
                        }

                        var gender = request.Query["Gender"];
                        Person person = new Person(name, dob, gender);
                        DateTime undefinedDate = new DateTime(1900, 01, 01);
                        if (name != "" && dob != undefinedDate && gender != "")
                            await response.WriteAsJsonAsync(person);
                        else
                        {
                            await response.WriteAsJsonAsync(new Person("undefined", undefinedDate, "undefined"));
                        }

                    }
                    else
                        await response.SendFileAsync("wwwroot/html/Index.html");
                }
                else if (path == "/about")
                {
                    await response.SendFileAsync("wwwroot/html/about.html");
                }
                else if (path == "/echo")
                {
                    await response.SendFileAsync("wwwroot/html/echo.html");
                }
                else
                {
                    response.StatusCode = 404;
                    await response.WriteAsync("Page doesn't exist");
                }

            });
            app.Run();
        }
    }
    public record Person(string Name, DateTime Dob, string Gender);
}

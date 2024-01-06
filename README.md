# Introduction
An example of using Asp.Net core Request pipeline.
In this case, to add a centralized exception handler.

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write?view=aspnetcore-7.0

# Running the project
You will need Net 7 Sdk.

Run the tests:

> dotnet test

Run the API:
> dotnet run

Click the link and navigate to Swagger
> http://localhost:5202/swagger/index.html (use the port shown in the link when you run the project)

## Motivation
Save time, makes code easier to test and read.

## Demo
We typically see controllers with code like this:

```
public IActionResult SomeEndPoint()
{
   try
   {
      // Return ok with the result payload
   }
   catch (Exception e)
   {
      // return bad request with the exception error message.
   }   
}
```
This requires testing both cases, for each single endpoint, resulting in many lines of code and hours of work.

In some cases, corrective actions are more complex than just returning a Bad Request response. In such cases, that complexity is 
duplicated in various end-points.

## Idea
We can achieve the same using middleware. In this sample project, there is a single controller with a single endpoint

```
[HttpGet("ping")]
public ActionResult<string> Ping()
{
  return Ok(_someService.Ping());
}
```

Note that, in practice, this controller behaves exactly the same as the example above (we moved the Exception handling part 
to a different place)

There are two integration tests that call this endpoint.

This one tests that when the code does not throw an exception, an Ok code with the response payload are returned
```
    [Fact]
    public async Task Ping_OnSuccess_ReturnsOkWithPongResponse()
    {
        // ************ ARRANGE ************
        
        var ep = "ping";

        var client = _sutFactory.CreateClient();

        // ************ ACT ****************

        var response = await client.GetAsync(ep);

        // ************ ASSERT *************
        
        Assert.True(response.IsSuccessStatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Equal("Pong!", responseContent);
    }

```

And this one tests that when the code throws an exception, a BadRequest response with the error message is returned
```
  [Fact]
    public async Task Pjng_OnError_RetunsBadRequestWithExceptionMessage()
    {

        // ************ ARRANGE ************

        using var scope = GetServiceFromDiContainer(
            out PingService pingService);
        // Setup the service so it will throw an exception.
        pingService.SetupToThrowException("Something went wrong!");

        var ep = "ping";

        var client = _sutFactory.CreateClient();

        // ************ ACT ****************

        var response = await client.GetAsync(ep);

        // ************ ASSERT *************
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Equal("Something went wrong!", responseContent);
      
    }
``` 

If you look into the code, this is implemented with a custom middleware class (found in the middleware folder) and added in program.cs

The custom middleware:

```
public class CustomExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public CustomExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
        catch (Exception e)
        {
            // This is just one example, in practice, the exception handling for an application will be more
            // complex. Sometimes the result is an error message, other times it is a different kind of corrective actions,
            // or sending an alert to an admin.
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(e.Message);
        }
    }
}
```

Then, there is an extension method to add the handler

```
public static class CustomExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder AddCustomExceptionHandlingMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomExceptionHandlingMiddleware>();
    }
}
```

And, finally, the handler is added in the Request pipeline in program.cs

```
app.AddCustomExceptionHandlingMiddleware();
```
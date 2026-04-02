// public static class ContextGen
// {
//     public static ActiveGeneratorContext GetContext()
//     {
//         LabOrder labOrder = new LabOrder("1", "Pediatrics");
//         Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

//         ConsoleLogger logger = new ConsoleLogger();
//         DataAccess testDataAccess = new DataAccess();



//         var services = new ServiceCollection();

//         Ember.Simulation.SandboxServiceCollectionExtensions.AddSandboxServices
//         (services, logger, testDataAccess);

//         using var provider = services.BuildServiceProvider();

//         ActiveContextFactory.IGeneratorContextFactory factory = provider.GetRequiredService<ActiveContextFactory.IGeneratorContextFactory>();


//         ActiveGeneratorContext ctx = factory.Create(labOrder, vaccine);

//         return ctx;
//     }
// }
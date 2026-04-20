namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;

[TestClass]
public class ServiceTest
{
    private DataService service;

    [TestInitialize]
    public void SetupBeforeEachTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdinationContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName: "test-database");
        var context = new OrdinationContext(optionsBuilder.Options);
        service = new DataService(context);
        service.SeedData();
    }

    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }

    [TestMethod]
    public void OpretDagligFast()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(2, service.GetDagligFaste().Count());
    }

    [TestMethod]
    public void GetAnbefaletDosisPerDoegn_LetPatient()
    {
        Patient patient = service.GetPatienter().First();
        patient.vaegt = 20; // gør patient "let"
    
        Laegemiddel lm = service.GetLaegemidler().First();
        
        double dosis = service.GetAnbefaletDosisPerDøgn(patient.PatientId, lm.LaegemiddelId);
        
        double forventet = patient.vaegt * lm.enhedPrKgPrDoegnLet;

        Assert.AreEqual(forventet, dosis);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestAtKodenSmiderEnException()
    {
        // Henter en gyldig patient og et gyldigt lægemiddel fra databasen
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        // Tjekker at der i starten kun findes 1 DagligFast i systemet
        Assert.AreEqual(1, service.GetDagligFaste().Count());

        
        // Her kaldes metoden med en ugyldig patientId (-1)
        // Det betyder at db.Patienter.Find(-1) returnerer null
        // Når koden derefter prøver at tilføje en ordination til patient,
        // vil det give en NullReferenceException
        service.OpretDagligFast(-1, lm.LaegemiddelId,
            2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));
        
        // Denne linje bliver aldrig nået, fordi testen forventer en exception
        // Hvis exception IKKE bliver kastet, vil testen fejle
        Assert.AreEqual(2, service.GetDagligFaste().Count());

        Console.WriteLine("Her kommer der ikke en exception. Testen fejler.");
    }
}
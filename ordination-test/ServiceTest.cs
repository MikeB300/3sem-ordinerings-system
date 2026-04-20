namespace ordination_test;

using Microsoft.EntityFrameworkCore;
using System;
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
        //testdata
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        // Tjek hvor mange DagligFast der findes før oprettelse
        int antalFor = service.GetDagligFaste().Count();
        
        //opret ny dagligfast
        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 1, 1, 0,
            DateTime.Today, DateTime.Today.AddDays(5));
        // Opret ordination der gælder i 5 dage (realistisk testperiode muligvis)
        
        //tjek at det er steget med 1
        int antalEfter = service.GetDagligFaste().Count();
        
        Assert.AreEqual(antalFor +1, antalEfter,
            "Der skulle være oprettet én ny DagligFast ordination");
    }
    [TestMethod]
    public void OpretDagligskaev()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Dosis[] doser = new Dosis[]
        {
            new Dosis(new DateTime(2026, 4, 20, 10, 0, 0), 1.5),
            new Dosis(new DateTime(2026, 4, 20, 14, 30, 0), 2.0),
            new Dosis(new DateTime(2026, 4, 20, 20, 0, 0), 0.5)
        };
        // Tjek hvor mange DagligSkæv der er før oprettelse
        int antalFor = service.GetDagligSkæve().Count();
        
        // Opret den nye DagligSkæv
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId,doser,
            DateTime.Today, DateTime.Today.AddDays(5));
        
        //tjekker antallet efter er steget med 1
        int antalEfter = service.GetDagligSkæve().Count();
        
        Assert.AreEqual(antalFor +1, antalEfter,
            "Der skulle være oprettet én ny DagligSkæv ordination");
    }
    
    [TestMethod]
    public void GetAnbefaletDosisPerDoegn_Let()
    {
        Patient patient = service.GetPatienter().First();
        patient.vaegt = 20; // gør patient "let"
    
        Laegemiddel lm = service.GetLaegemidler().First();
        
        double dosis = service.GetAnbefaletDosisPerDøgn(patient.PatientId, lm.LaegemiddelId);
        
        double forventet = patient.vaegt * lm.enhedPrKgPrDoegnLet;

        Assert.AreEqual(forventet, dosis);
    }
    
    [TestMethod]
    public void GetAnbefaletDosisPerDoegn_Normal()
    {
        Patient patient = service.GetPatienter().First();
        patient.vaegt = 25; // gør patient "normal"

        Laegemiddel lm = service.GetLaegemidler().First();

        double resultat = service.GetAnbefaletDosisPerDøgn(patient.PatientId, lm.LaegemiddelId);

        double forventet = 25 * lm.enhedPrKgPrDoegnNormal;

        Assert.AreEqual(forventet, resultat);
    }
    
    [TestMethod]
    public void GetAnbefaletDosisPerDoegn_Tung()
    {
        Patient patient = service.GetPatienter().First();
        patient.vaegt = 130; // gør patient "tung"

        Laegemiddel lm = service.GetLaegemidler().First();

        double resultat = service.GetAnbefaletDosisPerDøgn(patient.PatientId, lm.LaegemiddelId);

        double forventet = 130 * lm.enhedPrKgPrDoegnTung;

        Assert.AreEqual(forventet, resultat);
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
        //Assert.AreEqual(2, service.GetDagligFaste().Count());

        Console.WriteLine("Her kommer der ikke en exception. Testen fejler.");
    }
}
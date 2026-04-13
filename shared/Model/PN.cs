namespace shared.Model;

public class PN : Ordination {
	public double antalEnheder { get; set; }
    public List<Dato> dates { get; set; } = new List<Dato>();

    public PN (DateTime startDen, DateTime slutDen, double antalEnheder, Laegemiddel laegemiddel) : base(laegemiddel, startDen, slutDen) {
		this.antalEnheder = antalEnheder;
	}

    public PN() : base(null!, new DateTime(), new DateTime()) {
    }

    /// <summary>
    /// Registrerer at der er givet en dosis på dagen givesDen
    /// Returnerer true hvis givesDen er inden for ordinationens gyldighedsperiode og datoen huskes
    /// Returner false ellers og datoen givesDen ignoreres
    /// </summary>
    public bool givDosis(Dato givesDen) {
        
        return false;
    }

    public override double doegnDosis()
    {
	    // Find den tidligste og seneste dato hvor der er givet medicin
	    DateTime førsteDato = dates.Min(d => d.dato);
	    DateTime sidsteDato = dates.Max(d => d.dato);

	    // Beregn antal hele dage mellem første og sidste givning (begge inklusive)
	    int antalDage = (sidsteDato.Date - førsteDato.Date).Days + 1;

	    // Formlen fra opgaven
	    return (dates.Count * antalEnheder) / antalDage;
    }


    public override double samletDosis() {
        return dates.Count() * antalEnheder;
    }

    public int getAntalGangeGivet() {
        return dates.Count();
    }

	public override String getType() {
		return "PN";
	}
}

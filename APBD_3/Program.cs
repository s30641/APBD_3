class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

interface IHazardNotifier
{
    void PowiadomONiebezpieczenstwie(string wiadomosc);
}

abstract class Kontener
{
    private static int licznik  = 1;
    public string NumerSeryjny { get; }
    public double MaksymalnaLadownosc { get; }
    public double AktualnaLadownosc { get; protected set; }
    public double WagaWlasna { get; }
    public double  Wysokosc { get; }
    public double Glebokosc { get; }

    protected Kontener(string typ, double maksLadownosc, double wagaWlasna, double wysokosc, double glebokosc)
    {
        NumerSeryjny = $"KON-{typ}-{licznik++}";
        MaksymalnaLadownosc = maksLadownosc;
        WagaWlasna = wagaWlasna;
        Wysokosc =  wysokosc;
        Glebokosc = glebokosc;
        AktualnaLadownosc = 0;
    }

    public virtual void Zaladuj(double  masa)
    {
        if (AktualnaLadownosc + masa > MaksymalnaLadownosc)
            throw  new OverfillException("Przekroczono max ladonosc!");
        AktualnaLadownosc += masa;
    }

    public virtual void Oproznij()
    {
        AktualnaLadownosc = 0;
    }
    
    public override string ToString()
    {
        return $"{NumerSeryjny} - Waga wlasan: {WagaWlasna} kg, Ladunek: {AktualnaLadownosc}/{MaksymalnaLadownosc} kg";
    }
}

class KontenerChlodniczy : Kontener
{
    public string Produkt { get; }
    public double MinimalnaTemperatura { get; }

    public KontenerChlodniczy(double maksLadownosc, double wagaWlasna, double wysokosc, double glebokosc, string produkt, double minimalnaTemperatura)
        : base("C", maksLadownosc, wagaWlasna, wysokosc, glebokosc)
    {
        Produkt = produkt;
        MinimalnaTemperatura = minimalnaTemperatura;
    }

    public void Zaladuj(string produkt, double masa, double wymaganaTemperatura)
    {
        if (produkt != Produkt || wymaganaTemperatura > MinimalnaTemperatura)
            throw new  Exception("Nieprawidlowy produkt lub temp!");
        base.Zaladuj(masa);
    }
}

class KontenerNaPlyny : Kontener, IHazardNotifier
{
    public bool Niebezpieczny {  get; }

    public KontenerNaPlyny(double maksLadownosc, double wagaWlasna, double wysokosc, double glebokosc, bool niebezpieczny)
        : base("L", maksLadownosc, wagaWlasna, wysokosc, glebokosc)
    {
        Niebezpieczny = niebezpieczny;
    }

    public override void Zaladuj(double  masa)
    {
        double limit = Niebezpieczny ? MaksymalnaLadownosc * 0.5 : MaksymalnaLadownosc * 0.9;
        if (AktualnaLadownosc + masa > limit)
        {
            PowiadomONiebezpieczenstwie("Przeladowanie!");
            throw new OverfillException("Przekroczono limit!");
        }
        base.Zaladuj(masa);
    }

    public void PowiadomONiebezpieczenstwie(string  wiadomosc)
    {
        Console.WriteLine($"[UWAGA] {wiadomosc} - {NumerSeryjny}");
    }
}

class KontenerNaGaz : Kontener, IHazardNotifier
{
    public double Cisnienie  { get; }

    public KontenerNaGaz(double maksLadownosc, double wagaWlasna, double wysokosc, double glebokosc, double cisnienie)
        : base("G", maksLadownosc,  wagaWlasna, wysokosc, glebokosc)
    {
        Cisnienie = cisnienie;
    }

    public override void Oproznij()
    {
        AktualnaLadownosc *= 0.05;
    }

    public void PowiadomONiebezpieczenstwie(string wiadomosc)
    {
        Console.WriteLine($"[UWAGA] {wiadomosc} - {NumerSeryjny}");
    }
}

class Kontenerowiec
{
    public List<Kontener> Kontenery { get; } = new List<Kontener>();
    public double MaksymalnaLadownosc { get; }
    public double MaksymalnaPredkosc { get; }
    public int MaksymalnaIloscKontenerow { get; }

    public Kontenerowiec(int maksIlosc, double maksLadownosc, double maksPredkosc)
    {
        MaksymalnaIloscKontenerow = maksIlosc;
        MaksymalnaLadownosc =  maksLadownosc;
        MaksymalnaPredkosc = maksPredkosc;
    }

    public void ZaladujKontener(Kontener kontener)
    {
        if (Kontenery.Sum(k => k.AktualnaLadownosc + k.WagaWlasna) + kontener.AktualnaLadownosc + kontener.WagaWlasna > MaksymalnaLadownosc)

            throw new Exception("Przekroczona pojemnosc statku!");
        Kontenery.Add(kontener);
    }

    public void UsunKontener(string numerSeryjny)
    {
        Kontenery.RemoveAll(k => k.NumerSeryjny == numerSeryjny);
    }

    public void ZastapKontener(string numerSeryjny, Kontener nowyKontener)
    {
        UsunKontener (numerSeryjny);
        ZaladujKontener(nowyKontener);
    }
    
    public void WypiszInformacje()
    {
        Console.WriteLine($"Kontenerowiec: {Kontenery.Count} kontenerow, Maksymalna predkość: {MaksymalnaPredkosc}");
    }
    
    public void PrzeniesKontener(Kontenerowiec cel, string numerSeryjny)
    {
        var kontener = Kontenery.FirstOrDefault(k => k.NumerSeryjny == numerSeryjny);
        if (kontener != null)
        {
            UsunKontener (numerSeryjny);
            cel.ZaladujKontener(kontener);
        }
    }
    
    public void RozladujKontener(string numerSeryjny)
    {
        var kontener = Kontenery.FirstOrDefault(k => k.NumerSeryjny == numerSeryjny);
        kontener?.Oproznij( );
    }
}

class Program
{
    static void Main()
    {
        try
        {
            var statek1 = new Kontenerowiec(5, 15000, 25);
            var statek2 = new Kontenerowiec(3, 10000, 20);

            var gazowy = new KontenerNaGaz(5000, 800, 250, 600, 15);
            var chlodniczy = new KontenerChlodniczy(3000, 600, 250, 600, "mleko", 2);
            var plynyBezpieczne = new KontenerNaPlyny(4000, 700, 250, 600, false);
            var plynyNiebezpieczne = new KontenerNaPlyny(4000, 700, 250, 600, true);

            Console.WriteLine("\n=== Zaladunek kontenerow ===");
            gazowy.Zaladuj(2000);
            chlodniczy.Zaladuj("mleko", 1500, 1);
            plynyBezpieczne.Zaladuj(3500);

            statek1.ZaladujKontener(gazowy);
            statek1.ZaladujKontener(chlodniczy);
            statek1.ZaladujKontener(plynyBezpieczne);

            statek1.WypiszInformacje();

            Console.WriteLine("\n=== Test err kontenera na plyny ===");
            try
            {
                plynyNiebezpieczne.Zaladuj(2500);
            }
            catch (OverfillException ex)
            {
                Console.WriteLine($"Blad: {ex.Message}");
            }

            Console.WriteLine("\n=== Test oproznienia kontenerow ===");
            gazowy.Oproznij();
            Console.WriteLine(gazowy );

            Console.WriteLine("\n=== Usuwanie kontenera ===");
            statek1.UsunKontener(chlodniczy.NumerSeryjny);
            statek1.WypiszInformacje( );

            Console.WriteLine("\n=== Zastepowanie kontenera ===");
            var nowyChlodniczy = new KontenerChlodniczy(2500, 550, 250, 600, "ryby", -18);
            statek1.ZastapKontener(gazowy.NumerSeryjny, nowyChlodniczy);
            statek1.WypiszInformacje ();

            Console.WriteLine("\n=== Przenoszenie kontenera  na inny statek ===");
            statek1.PrzeniesKontener(statek2, nowyChlodniczy.NumerSeryjny);
            Console.WriteLine("\nStan statków po przeniesieniu:");
            statek1.WypiszInformacje();
            statek2.WypiszInformacje();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpil blad: {ex.Message}");
        }
    }

}

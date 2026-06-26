using System.Globalization;

namespace InfoGiovani_Back.Services;

//Servizio statico per la ricerca testuale "a 3 livelli":
//1. Corrispondenza esatta (parola == token)
//2. Corrispondenza per inizio o fine (parola è prefisso o suffisso del token)
//3. Corrispondenza interna (parola contenuta nel token, ma non a inizio/fine)
//Case-insensitive e accent-insensitive (replica via C# la collation del DB)
public static class RicercaTestualeService
{
    private static readonly CompareInfo Comparer = CultureInfo.InvariantCulture.CompareInfo;
    private const CompareOptions Opzioni = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    //Tier (1, 2 o 3) della corrispondenza tra una parola e un singolo token. Null se nessuna corrispondenza
    private static int? TierToken(string parola, string token)
    {
        if (Comparer.Compare(parola, token, Opzioni) == 0)
            return 1;

        if (parola.Length > token.Length)
            return null; //una parola più lunga del token non può esserne prefisso/suffisso/sottostringa

        bool inizio = Comparer.IsPrefix(token, parola, Opzioni);
        bool fine = Comparer.IsSuffix(token, parola, Opzioni);
        if (inizio || fine)
            return 2;

        if (Comparer.IndexOf(token, parola, Opzioni) >= 0)
            return 3;

        return null;
    }

    //Tier migliore (più basso) trovato cercando la parola tra tutti i token di un campo
    //(tokenizzazione solo sugli spazi). Null se la parola non è presente nel campo.

    public static int? TierParolaInCampo(string parola, string? campo)
    {
        if (string.IsNullOrWhiteSpace(campo))
            return null;

        int? migliore = null;
        foreach (var token in campo.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var tier = TierToken(parola, token);
            if (tier is int t && (migliore is null || t < migliore))
            {
                migliore = t;
                if (migliore == 1) break; //non si può fare meglio del tier 1
            }
        }
        return migliore;
    }

    //Tier migliore di una parola considerando più campi (OR tra campi).
    private static int? TierMigliore(string parola, IReadOnlyList<string?> campi)
    {
        int? migliore = null;
        foreach (var campo in campi)
        {
            var tier = TierParolaInCampo(parola, campo);
            if (tier is int t && (migliore is null || t < migliore))
                migliore = t;
        }
        return migliore;
    }

    //Calcola il punteggio complessivo di un record per una ricerca multi-parola.
    //- Il testo di ricerca viene tokenizzato sugli spazi.
    //- Ogni parola deve trovare corrispondenza (tier 1/2/3) in almeno uno dei campi passati (OR tra campi);
    //  se anche una sola parola non trova corrispondenza in nessun campo, il record viene escluso (AND tra parole) e si ritorna null.
    //- Il punteggio complessivo è la SOMMA dei tier delle singole parole: più basso = corrispondenza migliore.
    public static int? CalcolaPunteggio(string testoRicerca, params string?[] campi)
    {
        var parole = testoRicerca.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parole.Length == 0)
            return 0;

        int somma = 0;
        foreach (var parola in parole)
        {
            var tier = TierMigliore(parola, campi);
            if (tier is null)
                return null;
            somma += tier.Value;
        }
        return somma;
    }
    
    //Verifica se l'intera frase di ricerca compare come sottostringa consecutiva
    //(case/accent-insensitive, spazi multipli normalizzati a uno) in almeno uno dei campi.
    //Priorità massima: se trovata, il record deve precedere qualsiasi corrispondenza a parole sparse.
    public static bool TrovaFraseEsatta(string fraseRicerca, params string?[] campi)
    {
        var frase = NormalizzaSpazi(fraseRicerca);
        if (string.IsNullOrWhiteSpace(frase))
            return false;

        foreach (var campo in campi)
        {
            if (string.IsNullOrWhiteSpace(campo))
                continue;

            var campoNormalizzato = NormalizzaSpazi(campo);
            if (Comparer.IndexOf(campoNormalizzato, frase, Opzioni) >= 0)
                return true;
        }
        return false;
    }

    //Punteggio complessivo "a due livelli":
    //- frase esatta consecutiva trovata -> priorità massima (int.MinValue, batte qualsiasi somma di tier)
    //- altrimenti -> punteggio a somma di tier per parola (CalcolaPunteggio), null se non tutte le parole trovano corrispondenza
    public static int? CalcolaPunteggioTotale(string testoRicerca, params string?[] campi)
    {
        if (TrovaFraseEsatta(testoRicerca, campi))
            return int.MinValue;

        return CalcolaPunteggio(testoRicerca, campi);
    }

    private static string NormalizzaSpazi(string s) =>
        string.Join(' ', s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
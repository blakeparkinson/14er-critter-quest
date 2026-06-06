public enum PhotoRating
{
    NoCritter,
    Blurry,
    Decent,
    Majestic,
    NationalGeographic
}

public class PhotoResult
{
    public PhotoRating rating;
    public string ratingText;
    public int score;
    public CritterData primaryCritter;
    public int critterCount;
    public bool hadSillyAction;
}

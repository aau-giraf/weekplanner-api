namespace GirafAPI.Entities.Resources;

// Data model of pictograms in the database
public class Pictogram
{
    public int Id { get; set; }
    
    public Guid ImageId { get; set; }
    
    public string PictogramName { get; set; }
    
    //TODO: Add org when changes are merged
}
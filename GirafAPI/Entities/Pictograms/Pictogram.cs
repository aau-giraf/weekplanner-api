namespace GirafAPI.Entities.Pictograms;

// Data model of pictograms in the database
public class Pictogram
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public required Guid ImageId { get; set; }
    public required string PictogramName { get; set; }
}

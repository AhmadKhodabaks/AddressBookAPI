namespace AddressBook.API.Models
{
    public class GeoLocationResponse
{
    public string Class { get; set; }
    public string Type { get; set; }
    public string Licence { get; set; }
    public string Importance { get; set; }
    public string PlaceId { get; set; }
    public string OsmType { get; set; }
    public string OsmId { get; set; }
    public string DisplayName { get; set; }
    public List<string> Boundingbox { get; set; }
    public string Lat { get; set; }
    public string Lon { get; set; }
    public string PoweredBy { get; set; }
}


}

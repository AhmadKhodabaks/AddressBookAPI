using AddressBook.API.DataAccess;
using AddressBook.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AddressBook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AddressesController : ControllerBase
{
    private readonly AddressContext _context;

    public AddressesController(AddressContext context)
    {
        _context = context;
    }
    // GET: api/<AddressesController>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Address>>> GetAddresses(string search = null, string sortBy = null, bool ascending = true)
    {
        if (!string.IsNullOrEmpty(search) && !Regex.IsMatch(search, "^[a-zA-Z0-9]+$"))
        {
            return BadRequest("Search query parameter can only contain letters and numbers.");
        }

        var addresses = await _context.Addresses.ToListAsync();

        if (!string.IsNullOrEmpty(search))
        {
            var properties = typeof(Address).GetProperties();
            addresses = addresses.Where(a => properties.Any(p => p.GetValue(a)?.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            var property = typeof(Address).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                addresses = ascending ? addresses.OrderBy(a => property.GetValue(a, null)).ToList() : addresses.OrderByDescending(a => property.GetValue(a, null)).ToList();
            }
        }

        if (addresses.Count == 0)
        {
            return NotFound("No addresses found.");
        }

        return Ok(addresses);
    }

    [HttpGet("distance")]
    public async Task<ActionResult<Distance>> GetDistance(int addressId1, int addressId2)
    {
        var address1 = await _context.Addresses.FindAsync(addressId1);
        var address2 = await _context.Addresses.FindAsync(addressId2);

        if (address1 == null || address2 == null)
        {
            return NotFound();
        }

        try
        {
            // Make a request to the geolocation API to retrieve the latitude and longitude for each address
            var geoLocationServiceUrl = "https://geocode.maps.co/search?";

            var geoLocationRequestUrlA = $"{geoLocationServiceUrl}street={address1.Street}+{address1.HouseNumber}&city={address1.City}&country={address1.Country}&postalcode={address1.ZipCode}";
            var geoLocationRequestUrlB = $"{geoLocationServiceUrl}street={address2.Street}+{address2.HouseNumber}&city={address2.City}&country={address2.Country}&postalcode={address2.ZipCode}";

            var httpClient = new HttpClient();
            var geoLocationResponseA = await httpClient.GetFromJsonAsync<GeoLocationResponse>(geoLocationRequestUrlA);
            var geoLocationResponseB = await httpClient.GetFromJsonAsync<GeoLocationResponse>(geoLocationRequestUrlB);

            //if (geoLocationResponseA.Status != "OK" || geoLocationResponseB.Status != "OK")
            //{
            //    return BadRequest("Invalid addresses provided");
            //}

            //var locationA = geoLocationResponseA.Results.FirstOrDefault()?.Geometry.Location;
            //var locationB = geoLocationResponseB.Results.FirstOrDefault()?.Geometry.Location;

            //if (locationA == null || locationB == null)
            //{
            //    return BadRequest("Invalid addresses provided");
            //}

            // Calculate the distance between the two addresses using the Haversine formula
            var distanceInKm = CalculateDistanceInKm(Double.Parse(geoLocationResponseA.Lat), Double.Parse(geoLocationResponseA.Lon), Double.Parse(geoLocationResponseB.Lat), Double.Parse(geoLocationResponseB.Lon));


            // Return the distance in kilometers
            return new Distance
            {
                Origin = $"{address1.Street}, {address1.HouseNumber}, {address1.ZipCode}, {address1.City},{address1.Country}",
                Destination = $"{address2.Street}, {address2.HouseNumber}, {address2.ZipCode}, {address2.City},{address2.Country}",
                DistanceInKm = distanceInKm
            };
        }
        catch (JsonException)
        {
            return BadRequest("Invalid response from geolocation API");
        }
        catch (HttpRequestException)
        {
            return BadRequest("Unable to connect to geolocation API");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    private static double CalculateDistanceInKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Radius of the earth in km
        var dLat = Deg2Rad(lat2 - lat1);
        var dLon = Deg2Rad(lon2 - lon1);
        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var d = R * c; // Distance in km
        return d;
    }

    private static double Deg2Rad(double deg)
    {
        return deg * (Math.PI / 180);
    }





    // GET api/<AddressesController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Address>> GetAddress(int id)
    {
        var address = await _context.Addresses.FindAsync(id);

        if (address == null)
        {
            return NotFound();
        }

        return address;
    }

    // POST api/<AddressesController>
    [HttpPost]
    public async Task<ActionResult<Address>> PostAddress(Address address)
    {
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
    }

    // PUT api/<AddressesController>/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAddress(int id, Address address)
    {
        if (id != address.Id)
        {
            return BadRequest();
        }

        _context.Entry(address).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AddressExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE api/<AddressesController>/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var address = await _context.Addresses.FindAsync(id);
        if (address == null)
        {
            return NotFound();
        }

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    private bool AddressExists(int id)
    {
        return _context.Addresses.Any(e => e.Id == id);
    }
}

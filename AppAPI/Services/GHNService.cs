
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AppAPI.Services
{
    public class GHNService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly int _shopId;
        private readonly int _fromDistrict;

        public GHNService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;

            string token = config["GHN:Token"]!;
            _httpClient.BaseAddress = new Uri("https://online-gateway.ghn.vn/shiip/public-api/");
            _httpClient.DefaultRequestHeaders.Add("Token", token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _shopId = int.Parse(config["GHN:ShopId"]!);
            _fromDistrict = int.Parse(config["GHN:FromDistrictId"]!);
        }

        public async Task<List<GHNProvince>> GetAllProvinces()
        {
            var response = await _httpClient.GetAsync("master-data/province");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GHNResponse<List<GHNProvince>>>(content);

            return result?.Data ?? new List<GHNProvince>();
        }

        public async Task<List<object>> GetDistrictsByProvinceId(int provinceId)
        {
            var body = new { province_id = provinceId };
            var response = await _httpClient.PostAsJsonAsync("https://online-gateway.ghn.vn/shiip/public-api/master-data/district", body);
            var content = await response.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(content);
            var data = json.RootElement.GetProperty("data");

            return data.EnumerateArray().Select(d => new {
                DistrictID = d.GetProperty("DistrictID").GetInt32(),
                DistrictName = d.GetProperty("DistrictName").GetString()
            }).ToList<object>();
        }

        public async Task<List<object>> GetWardsByDistrictId(int districtId)
        {
            var body = new { district_id = districtId };
            var response = await _httpClient.PostAsJsonAsync("https://online-gateway.ghn.vn/shiip/public-api/master-data/ward", body);

            if (!response.IsSuccessStatusCode)
                return new List<object>();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var data = json.RootElement.GetProperty("data");

            return data.EnumerateArray().Select(w => new
            {
                WardCode = w.GetProperty("WardCode").GetString(),
                WardName = w.GetProperty("WardName").GetString()
            }).ToList<object>();
        }



        public async Task<List<GHNDistrict>> GetDistricts(int provinceId)
        {
            var body = new { province_id = provinceId };
            var response = await _httpClient.PostAsJsonAsync("master-data/district", body);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GHNResponse<List<GHNDistrict>>>(content);

            return result?.Data ?? new List<GHNDistrict>();
        }

        public async Task<List<GHNWard>> GetWards(int districtId)
        {
            var body = new { district_id = districtId };
            var response = await _httpClient.PostAsJsonAsync("master-data/ward", body);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GHNResponse<List<GHNWard>>>(content);

            return result?.Data ?? new List<GHNWard>();
        }

        public async Task<int?> GetProvinceIdByName(string name)
        {
            var provinces = await GetAllProvinces();
            var province = provinces.FirstOrDefault(p => p.ProvinceName.Equals(name, StringComparison.OrdinalIgnoreCase));
            return province?.ProvinceId;
        }

        public async Task<int?> GetDistrictIdByName(string name, int provinceId)
        {
            var districts = await GetDistricts(provinceId);
            var district = districts.FirstOrDefault(d => d.DistrictName.Equals(name, StringComparison.OrdinalIgnoreCase));
            return district?.DistrictId;
        }

        public async Task<string?> GetWardCode(string name, int districtId)
        {
            var wards = await GetWards(districtId);
            var ward = wards.FirstOrDefault(w => w.WardName.Equals(name, StringComparison.OrdinalIgnoreCase));
            return ward?.WardCode;
        }

        public async Task<ShippingFeeResult> CalculateShippingFee(int toDistrictId, string toWardCode, int insuranceValue, string? coupon = null)
        {
            var payload = new
            {
                service_type_id = 2,
                insurance_value = insuranceValue,
                coupon = coupon,
                from_district_id = _fromDistrict,
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                weight = 1000,
                length = 30,
                width = 20,
                height = 10
            };

            var response = await _httpClient.PostAsJsonAsync("v2/shipping-order/fee", payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new ShippingFeeResult { Success = false, ErrorMessage = content };

            var result = JsonConvert.DeserializeObject<GHNResponse<ShippingFeeDetail>>(content);

            if (result == null || result.Data == null)
                return new ShippingFeeResult { Success = false, ErrorMessage = "Không tính được phí vận chuyển" };

            return new ShippingFeeResult
            {
                Success = true,
                TotalFee = result.Data.Total,
                ServiceFee = result.Data.ServiceFee,
                InsuranceFee = result.Data.InsuranceFee,
                PickStationFee = result.Data.PickStationFee,
                CouponValue = result.Data.CouponValue
            };
        }
    }

    // DTOs
    public class GHNResponse<T>
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("data")]
        public T? Data { get; set; }
    }

    public class GHNProvince
    {
        [JsonProperty("ProvinceID")]
        public int ProvinceId { get; set; }

        [JsonProperty("ProvinceName")]
        public string ProvinceName { get; set; } = "";
    }

    public class GHNDistrict
    {
        [JsonProperty("DistrictID")]
        public int DistrictId { get; set; }

        [JsonProperty("DistrictName")]
        public string DistrictName { get; set; } = "";
    }

    public class GHNWard
    {
        [JsonProperty("WardCode")]
        public string WardCode { get; set; } = "";

        [JsonProperty("WardName")]
        public string WardName { get; set; } = "";
    }

    public class ShippingFeeResult
    {
        public bool Success { get; set; }
        public int TotalFee { get; set; }
        public int ServiceFee { get; set; }
        public int InsuranceFee { get; set; }
        public int PickStationFee { get; set; }
        public int CouponValue { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ShippingFeeDetail
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("service_fee")]
        public int ServiceFee { get; set; }

        [JsonProperty("insurance_fee")]
        public int InsuranceFee { get; set; }

        [JsonProperty("pick_station_fee")]
        public int PickStationFee { get; set; }

        [JsonProperty("coupon_value")]
        public int CouponValue { get; set; }
    }
}

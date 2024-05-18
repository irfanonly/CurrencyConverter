
using CurrencyConverter.WebAPI.Models;
using System.Text.Json;

namespace CurrencyConverter.UnitTest.Controllers
{
    public class CurrencyControllerTests
    {
        private readonly Mock<IExchangeService> _exchangeServiceMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<CurrencyController>> _loggerMock;
        private readonly CurrencyController _controller;

        public CurrencyControllerTests()
        {
            _exchangeServiceMock = new Mock<IExchangeService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<CurrencyController>>();
            _configurationMock = new Mock<IConfiguration>();
            _controller = new CurrencyController(_exchangeServiceMock.Object, _loggerMock.Object, _configurationMock.Object,  _cacheServiceMock.Object);
        }

        #region GetLatestExchangeRates

        [Fact]
        public async Task GetLatestExchangeRates_ShouldReturnBadRequest_WhenBaseCurrencyIsInvalid()
        {
            // Act
            var result = await _controller.GetLatestExchangeRates("EU");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("The currency code should be in 3 characters", badRequestResult.Value);
        }

        [Fact]
        public async Task GetLatestExchangeRates_ShouldReturnNotFound_WhenExchangeServiceReturnsNull()
        {
            // Arrange
            var baseCurrency = "EUR";
            var cacheKey = $"GetLatestExchangeRates_{baseCurrency}";

            _cacheServiceMock.Setup(c => c.GetOrSetCacheAsync(cacheKey, It.IsAny<Func<Task<string>>>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync((string)null);

            // Act
            var result = await _controller.GetLatestExchangeRates(baseCurrency);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetLatestExchangeRates_ShouldReturnOk_WhenExchangeServiceReturnsData()
        {
            // Arrange
            var baseCurrency = "EUR";
            var cacheKey = $"GetLatestExchangeRates_{baseCurrency}";
            var expectedData = "{\"amount\":1.0,\"base\":\"EUR\",\"date\":\"2024-05-17\",\"rates\":{\"AUD\":1.6281,\"BGN\":1.9558,\"BRL\":5.5645,\"CAD\":1.4784}}";

            _cacheServiceMock.Setup(c => c.GetOrSetCacheAsync(cacheKey, It.IsAny<Func<Task<string>>>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(expectedData);

            // Act
            var result = await _controller.GetLatestExchangeRates(baseCurrency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedData, okResult.Value);
        }

        [Fact]
        public async Task GetLatestExchangeRates_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var baseCurrency = "EUR";
            var cacheKey = $"GetLatestExchangeRates_{baseCurrency}";

            _cacheServiceMock.Setup(c => c.GetOrSetCacheAsync(cacheKey, It.IsAny<Func<Task<string>>>(), It.IsAny<TimeSpan>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetLatestExchangeRates(baseCurrency);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }

        #endregion

        #region ConvertAmount

        [Fact]
        public async Task ConvertAmount_ShouldReturnBadRequest_WhenIsInvalid()
        {
            // Arrange
            _configurationMock.SetupGet(x => x["CONVERT_EXCLUSION_LIST"])
                              .Returns("[\"TRY\",\"PLN\",\"THB\",\"MXN\"]");

            // Act
            var result1 = await _controller.ConvertAmount(-10);
            var result2 = await _controller.ConvertAmount(fromCurrency: "UR");
            var result3 = await _controller.ConvertAmount(toCurrency: "TRY");
            var result4 = await _controller.ConvertAmount(toCurrency: "UR");
            var result5 = await _controller.ConvertAmount(toCurrency: "PLN");


            // Assert
            var badRequestResult1 = Assert.IsType<BadRequestObjectResult>(result1);
            var badRequestResult2 = Assert.IsType<BadRequestObjectResult>(result2);
            var badRequestResult3 = Assert.IsType<BadRequestObjectResult>(result3);
            var badRequestResult4 = Assert.IsType<BadRequestObjectResult>(result4);
            var badRequestResult5 = Assert.IsType<BadRequestObjectResult>(result5);
            Assert.Equal("The amount should be greater than Zero(0)", badRequestResult1.Value);
            Assert.Equal("The fromCurrency should be in 3 characters", badRequestResult2.Value);
            Assert.Equal("The currency TRY is not allowed for conversion", badRequestResult3.Value);
            Assert.Equal("The toCurrency should be in 3 characters", badRequestResult4.Value);
            Assert.Equal("The currency PLN is not allowed for conversion", badRequestResult5.Value);

        }

        [Fact]
        public async Task ConvertAmount_ShouldReturnNotFound_WhenServiceReturnsNull()
        {
            // Arrange
            decimal amount = 1;
            string fromCurrency = "EUR";
            string toCurrency = "USD";

            _exchangeServiceMock.Setup(x => x.Convert(amount, fromCurrency, toCurrency)).ReturnsAsync((string)null);

            // Act
            var result = await _controller.ConvertAmount(amount, fromCurrency, toCurrency);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ConvertAmount_ShouldReturnOk_WhenServiceReturnsData()
        {
            // Arrange
            decimal amount = 1;
            string fromCurrency = "EUR";
            string toCurrency = "USD";
            string expectedData = "{\"amount\":1.0,\"base\":\"EUR\",\"date\":\"2024-05-17\",\"rates\":{\"USD\":1.0844}}";

            string cacheKey = $"ConvertAmount_{amount}_{fromCurrency}_{toCurrency}";
            _cacheServiceMock.Setup(c => c.GetOrSetCacheAsync(cacheKey , It.IsAny<Func<Task<string>>>(), It.IsAny<TimeSpan>()))
               .ReturnsAsync(expectedData);
           

            // Act
            var result = await _controller.ConvertAmount(amount, fromCurrency, toCurrency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedData, okResult.Value);
        }


        [Fact]
        public async Task ConvertAmount_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            decimal amount = 1;
            string fromCurrency = "EUR";
            string toCurrency = "USD";
            string cacheKey = $"ConvertAmount_{amount}_{fromCurrency}_{toCurrency}";

            _cacheServiceMock.Setup(c => c.GetOrSetCacheAsync(cacheKey, It.IsAny<Func<Task<string>>>(), It.IsAny<TimeSpan>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.ConvertAmount();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }



        #endregion

        #region History

        [Fact]
        public async Task History_ShouldReturnBadRequest_WhenIsInvalid()
        {
            // Arrange
            string fromDate = "2024-13-01"; // Invalid date
            string toDate = "2024-05-32"; // Invalid date
            string currency = "AUDCAD"; // Invalid currency

            // Act
            var result1 = await _controller.History(fromDate: fromDate);
            var result2 = await _controller.History(toDate: toDate);
            var result3 = await _controller.History(currency: currency);

            // Assert
            var badRequestResult1 = Assert.IsType<BadRequestObjectResult>(result1);
            var badRequestResult2 = Assert.IsType<BadRequestObjectResult>(result2);
            var badRequestResult3 = Assert.IsType<BadRequestObjectResult>(result3);
            Assert.Equal($"The {fromDate} is not valid", badRequestResult1.Value);
            Assert.Equal($"The {toDate} is not valid", badRequestResult2.Value);
            Assert.Equal("The currency code should be in 3 characters", badRequestResult3.Value);
        }

        [Fact]
        public async Task History_ShouldReturnBadRequest_WhenFromDateIsGreaterThanToDate()
        {
            // Arrange
            string fromDate = "2024-05-20";
            string toDate = "2024-05-15"; // fromDate > toDate

            // Act
            var result = await _controller.History(fromDate: fromDate, toDate: toDate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("'to' date should be greater than 'from' date", badRequestResult.Value);
        }

        [Fact]
        public async Task History_ShouldReturnNotFound_WhenServiceReturnsNull()
        {
            // Arrange
            string fromDate = "2024-05-01";
            string toDate = "2024-05-17";
            string currency = "AUD";
            string cacheKey = $"History_{fromDate}_{toDate}_{currency}";

            _cacheServiceMock.Setup(c => c.GetOrSetCacheAsync(cacheKey, It.IsAny<Func<Task<string>>>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync((string)null);

            // Act
            var result = await _controller.History(fromDate, toDate, currency);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task History_ShouldReturnOk_WhenServiceReturnsData()
        {
            // Arrange
            string fromDate = "2024-05-01";
            string toDate = "2024-05-17";
            string currency = "AUD";
            string cacheKey = $"History_{fromDate}_{toDate}_{currency}";

            

            var expectedData = new CurrencyRates
            {
                Rates = new Dictionary<DateTime, Dictionary<string, decimal>>
            {
                { new DateTime(2024, 5, 1), new Dictionary<string, decimal>{ { "AUD", 1.2m} } },
                { new DateTime(2024, 5, 2), new Dictionary<string, decimal>{ { "AUD", 1.2m} } },
                { new DateTime(2024, 5, 3), new Dictionary<string, decimal>{ { "AUD", 1.2m} } },
                { new DateTime(2024, 5, 4), new Dictionary<string, decimal>{ { "AUD", 1.2m} } },
                { new DateTime(2024, 5, 5), new Dictionary<string, decimal>{ { "AUD", 1.2m} } }

            }
            };

            var take1 = expectedData.Rates.Take(1);

            _cacheServiceMock.Setup(c => c.GetOrSetCacheAsync(cacheKey, It.IsAny<Func<Task<CurrencyRates>>>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(expectedData);

            // Act
            var result = await _controller.History(fromDate, toDate, currency, 1, 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var jsonResult = JsonSerializer.Serialize(okResult.Value);

            Assert.Equal(JsonSerializer.Serialize(take1), jsonResult);
        }

        #endregion
    }
}

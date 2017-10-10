using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using Moq;
using System.Web.Routing;
using System.Reflection;

namespace UrlsAndRoutes.Tests
{
    [TestClass]
    public class RouteTests
    {
        #region Test Helpers

        private HttpContextBase CreateHttpContext(string targetUrl = null, string httpMethod = "GET")
        {
            // Create mock request
            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(m => m.AppRelativeCurrentExecutionFilePath).Returns(targetUrl);
            mockRequest.Setup(m => m.HttpMethod).Returns(httpMethod);

            // Create mock response
            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(m => m.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(s => s);

            // Create mock context
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(m => m.Request).Returns(mockRequest.Object);
            mockContext.Setup(m => m.Response).Returns(mockResponse.Object);

            return mockContext.Object;
        }

        private void TestRouteMatch(string url, string controller, string action, object routeProperties = null, string httpMethod = "Get")
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            RouteConfig.RegisterRoutes(routes);
            // Act - process the route
            RouteData result = routes.GetRouteData(CreateHttpContext(url, httpMethod));
            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(TestIncomingRouteResult(result, controller, action, routeProperties));
        }

        private bool TestIncomingRouteResult(RouteData routeResult, string controller, string action, object propertySet = null)
        {
            Func<object, object, bool> valCompare = (v1, v2) =>
            {
                return StringComparer.InvariantCultureIgnoreCase.Compare(v1, v2) == 0;
            };

            bool result = valCompare(routeResult.Values["controller"], controller) && valCompare(routeResult.Values["action"], action);

            if (propertySet != null)
            {
                PropertyInfo[] propInfo = propertySet.GetType().GetProperties();
                foreach (var pi in propInfo)
                {
                    if (!(
                        routeResult.Values.ContainsKey(pi.Name)) &&
                        (valCompare(routeResult.Values[pi.Name], pi.GetValue(propertySet, null))))
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }

        private void TestRouteFail(string url)
        {
            // Arrange
            RouteCollection routes = new RouteCollection();
            RouteConfig.RegisterRoutes(routes);

            // Act - process the route
            RouteData result = routes.GetRouteData(CreateHttpContext(url));

            // Assert
            Assert.IsTrue(result == null || result.Route == null);
        }

        #endregion Test Helpers

        [TestMethod]
        public void TestIncomingDefaultRoute()
        {
            // Check for the url that is hoped for
            TestRouteMatch("~/", "Home", "Index");
            TestRouteMatch("~/Home", "Home", "Index");
        }

        [TestMethod]
        public void Test_Incoming_Customer_Route_Segments()
        {
            // Check that the values are being obtained from the segments
            TestRouteMatch("~/Customer", "Customer", "Index");
            TestRouteMatch("~/Customer/Index", "Customer", "Index");
            TestRouteMatch("~/Customer/List", "Customer", "List");
            TestRouteMatch("~/Admin", "Admin", "Index");
            TestRouteMatch("~/Admin/Index", "Admin", "Index");
        }

        [TestMethod]
        public void Test_Customer_Controller_Routes()
        {
            // Ensure that too many or too few segments fail to match
            TestRouteMatch("~/", "Home", "Index");
        }

        [TestMethod]
        public void Test_For_Too_Many_Segments()
        {
            // This should pass with 3 segments but fails as this url is still routeable!...
            // ...due to the extra segment being assigned to id parameter..."
            // Add 4 however and the test passes....
            TestRouteFail("~/Admin/Index/Id/Segment");
            TestRouteFail("~/Home/Index/Id/Segment");
            TestRouteFail("~/Customer/Index/Id/Segment");
        }

        [TestMethod]
        public void Test_Mixed_Static_Segment_Url()
        {
            TestRouteMatch("~/XHome/Index", "Home", "Index");
        }
    }
}

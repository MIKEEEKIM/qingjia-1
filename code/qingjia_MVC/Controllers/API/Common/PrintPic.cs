using qingjia_MVC.Models.API;
using System.Web.Http;

namespace qingjia_MVC.Controllers.API.Common
{
    [RoutePrefix("api/common")]
    public class Print : BaseApiController
    {
        [HttpGet, Route("print")]
        public ApiResult PrintPic(string access_token, string LL_ID)
        {

            return null;
        }

        
    }
}
namespace Saas.UserManage.Models
{
    class RequestInfo
    {
        private dynamic data;

        public RequestInfo(dynamic data)
        {
            this.data = data;
        }

        public string Country
        {
            get
            {
                return data.country_name;
            }
        }
    }
}
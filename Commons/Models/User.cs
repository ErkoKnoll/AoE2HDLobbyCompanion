using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aoe2LobbyListFetcherService.Models {
    public class User {
        public string ProfileUrl { get; set; }
        public string LocCountryCode { get; set; }
        public int CommunityVisibilityState { get; set; }
    }
}

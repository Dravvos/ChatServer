using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DTO
{
    public enum EnumChatType
    {
        Private,
        Group
    }
    public class ChatDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public EnumChatType ChatType { get; set; }
        public List<UserDTO> Participants { get; set; } = new List<UserDTO>();
        public Guid UserAdminId { get; set; }
    }
}

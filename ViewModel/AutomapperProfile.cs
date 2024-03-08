using AutoMapper;
using static TestFinal.ViewModel.ViewModels;
using TestFinal.Models;

namespace TestFinal.ViewModel
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<Post, PostDTO>();
            CreateMap<User, UserDTO>();
            CreateMap<Comment, CommentDTO>();
            CreateMap<Request, RequestDTO>();
            CreateMap<Like, LikeDTO>();
            CreateMap<Message, MessageDTO>();
            CreateMap<ChatRoom, ChatRoomDTO>();
            CreateMap<RoomMessage, RoomMessageDTO>();



        }

    }
}

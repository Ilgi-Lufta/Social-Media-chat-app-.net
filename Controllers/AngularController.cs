using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.Extensions.Hosting;
using static TestFinal.ViewModel.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace TestFinal.Controllers;

public class AngularController : Controller
{
    private readonly ILogger<AngularController> _logger;
    private readonly IWebHostEnvironment WebHostEnvironment;
    private readonly IMapper _mapper;

    private MyContext _context;

    // here we can "inject" our context service into the constructor
    public AngularController(ILogger<AngularController> logger, MyContext context, IWebHostEnvironment webHostEnvironment, IMapper mapper)
    {
        _logger = logger;
        _context = context;
        WebHostEnvironment = webHostEnvironment;
        _mapper = mapper;

    }
    public async Task<IActionResult> Index(string searchString)

    {
        // Pjesa me request

        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }
        int id = (int)HttpContext.Session.GetInt32("userId");
        User useriLoguar = _context.Users.FirstOrDefault(e => e.UserId == id);
        ViewBag.id = id;
        ViewBag.iLoguari = useriLoguar;

        ViewBag.Allusers = _context.Users
            .Include(e => e.RequestsReciver)
            .Include(e => e.RequestsSender)
            .Where(e => e.UserId != id)
            .Where(e =>
                         (e.RequestsSender.Any(f => f.ReciverId == id) == false)
                        && (e.RequestsReciver.Any(f => f.SenderId == id) == false)
            ).Take(3)
            .ToList();

        //ViewBag.Allusers = _context.Users
        //    .Include(e => e.RequestsReciver)
        //    .Include(e => e.RequestsSender)
        //    .Where(e => e.UserId != id)
        //    .Where(e =>e.RequestsSender

        ViewBag.request = _context.Requests.Include(e => e.Reciver).Include(e => e.Sender)
                            .Where(e => e.ReciverId == id)
                            .Where(e => e.Accepted == false)
                            .ToList();

        ViewBag.friends = _context.Requests.Include(e => e.Reciver).Include(e => e.Sender)
                            .Where(e => (e.ReciverId == id) || (e.SenderId == id))
                            .Where(e => e.Accepted == true)
                            .ToList();

        ViewBag.posts = _context.Posts.Include(e => e.Creator).Include(e => e.Likes).Include(e => e.Comments)
                                        .ThenInclude(e => e.UseriQekomenton).ThenInclude(e => e.RequestsReciver)
                                        .OrderByDescending(e => e.CreatedAt)

                                        .Where(e => (e.Creator.RequestsSender.Where(e => e.Accepted == true).Any(e => e.ReciverId == id) == false)
                                        || (e.Creator.RequestsReciver.Where(e => e.Accepted == true).Any(e => e.SenderId == id) == false)
                                        || e.Creator.UserId == id)
                                        .ToList();



        var searchfrineds = from m in _context.Users
                            select m;
        ViewBag.searchfrineds = searchfrineds.Take(3);

        if (!String.IsNullOrEmpty(searchString))
        {
            ViewBag.searchfrineds = searchfrineds.Where(s => s.FirstName!.Contains(searchString));
        }



        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("AngularRegister")]
    public IActionResult Register()
    {


        if (HttpContext.Session.GetInt32("userId") == null)
        {

            return View();
        }

        return RedirectToAction("Index");

    }
    [HttpPost("AngularRegister")]
    public IActionResult Register([FromBody] User user)
    {

        // If a User exists with provided email
        if (_context.Users.Any(u => u.UserName == user.UserName))
        {
            // Manually add a ModelState error to the Email field, with provided
            // error message
            ModelState.AddModelError("UserName", "UserName already in use!");

            return View();
            // You may consider returning to the View at this point
        }
        PasswordHasher<User> Hasher = new PasswordHasher<User>();
        user.Password = Hasher.HashPassword(user, user.Password);
        _context.Users.Add(user);
        _context.SaveChanges();
        HttpContext.Session.SetInt32("userId", user.UserId);
        user.Token = "Token";
        user.ExpiresIn = DateTime.Now.AddMinutes(60);
        return Ok(user);

    }

    [HttpPost("AngularLogin")]
    public IActionResult LoginSubmit([FromBody] LoginUser userSubmission)
    {
        if (ModelState.IsValid)
        {
            // If initial ModelState is valid, query for a user with provided email
            var userInDb = _context.Users.FirstOrDefault(u => u.UserName == userSubmission.UserName);
            // If no user exists with provided email
            if (userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("User", "Invalid UserName/Password");
                return View("Register");
            }

            // Initialize hasher object
            var hasher = new PasswordHasher<LoginUser>();

            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

            // result can be compared to 0 for failure
            if (result == 0)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                return View("Register");
                // handle failure (this should be similar to how "existing email" is handled)
            }
            HttpContext.Session.SetInt32("userId", userInDb.UserId);

            // return RedirectToAction("Index");
            userInDb.Token = "Token";
            userInDb.ExpiresIn = DateTime.Now.AddMinutes(60);
            return Ok(userInDb);
        }

        //     return View("Register");
        return BadRequest();
    }

    [HttpGet("Angularlogout")]
    public IActionResult Logout()
    {

        HttpContext.Session.Clear();
        return RedirectToAction("register");
    }


    // Pjesa me request


    [HttpGet("AngularSendR/{id}/{userId}")]
    public IActionResult SendR(int id, int userId)
    {
        //int idFromSession = (int)HttpContext.Session.GetInt32("userId");
        if (_context.Requests.Any(u => (u.SenderId == userId) && (u.ReciverId == id)))
        {

            return BadRequest();
        }
        else {

            Request newRequest = new Models.Request()
            {
                SenderId = userId,
                ReciverId = id,

            };
            _context.Requests.Add(newRequest);
            _context.SaveChanges();

            return Ok(newRequest);
        }


    }
    [HttpGet("AngularAcceptR/{id}")]
    public IActionResult AcceptR(int id)
    {

        Request requesti = _context.Requests.First(e => e.RequestId == id);
        requesti.Accepted = true;

        _context.SaveChanges();
        return Ok(requesti);
    }
    [HttpGet("AngularDeclineR/{id}")]
    public IActionResult Decline(int id)
    {

        Request requesti = _context.Requests.First(e => e.RequestId == id);
        _context.Remove(requesti);
        _context.SaveChanges();
        return Ok(requesti);
    }
    [HttpGet("AngularRemoveF/{id}")]
    public IActionResult RemoveF(int id)
    {

        Request requesti = _context.Requests.First(e => e.RequestId == id);
        _context.Remove(requesti);
        _context.SaveChanges();
        return Ok(requesti);
    }

    //Perfundon Request

    //pjesa me poste

    public IActionResult PostAdd(int id)
    {
        ViewBag.id = id;
        return View();
    }

    [HttpPost("PostCreate/{id}")]
    public IActionResult PostCreate(IFormFile File,string Description, int id)
    {

       var test= Request.Form;
        string StringFileName = UploadFile(File);
        var post = new Post()
        {
            Description = Description,
            Myimage = StringFileName
        };

        post.UserId = id;
        _context.Posts.Add(post);
        _context.SaveChanges();
        return Ok(post);
    }

    private string UploadFile(IFormFile image)
    {
        string fileName = null;
        if (image != null) {
            string Uploaddir = Path.Combine(WebHostEnvironment.WebRootPath, "Images");
            fileName = Guid.NewGuid().ToString() + "-" + image.FileName;
            string filePath = Path.Combine(Uploaddir, fileName);
            using (var filestream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(filestream);
            }
        }
        return fileName;
    }
    private string UploadFile(User marrNgaView)
    {
        string fileName = null;
        if (marrNgaView.Image != null) {
            string Uploaddir = Path.Combine(WebHostEnvironment.WebRootPath, "Images");
            fileName = Guid.NewGuid().ToString() + "-" + marrNgaView.Image.FileName;
            string filePath = Path.Combine(Uploaddir, fileName);
            using (var filestream = new FileStream(filePath, FileMode.Create))
            {
                marrNgaView.Image.CopyTo(filestream);
            }
        }
        return fileName;
    }
    [HttpGet("Like/{id}/{id2}")]
    public IActionResult Like(int id, int id2)
    {
        Like dblike = _context.Likes.FirstOrDefault(e => (e.UserId == id) && (e.PostId == id2));
        if (dblike == null) {
            Like mylike = new Like()
            {
                UserId = id,
                PostId = id2
            };
            _context.Add(mylike);
            _context.SaveChanges();
            return Ok(mylike);
        }
        return BadRequest();
    }
    [HttpGet("UnLike/{id}/{id2}")]
    public IActionResult UnLike(int id, int id2)
    {
        Like dblike = _context.Likes.FirstOrDefault(e => (e.UserId == id) && (e.PostId == id2));
        if (dblike != null)
        {
            _context.Remove(dblike);
            _context.SaveChanges();
            return Ok(dblike);
        }
        return BadRequest();
    }

    [HttpPost("CommentCreate/{id}/{id2}/{content}")]
    public IActionResult CommentCreate(int id, int id2, string content)
    {


        Comment comment = new Comment()
        {
            UserId = id,
            PostId = id2,
            content = content
        };
        _context.Add(comment);
        _context.SaveChanges();
        return Ok(comment);


    }

    public IActionResult Profilepicadd()
    {

        return View();
    }
    public IActionResult Profilepicsave(User marrNgaView)
    {
        int? id = HttpContext.Session.GetInt32("userId");
        string StringFileName = UploadFile(marrNgaView);

        User editUser = _context.Users.First(e => e.UserId == id);
        editUser.Myimage = StringFileName;
        _context.SaveChanges();
        return RedirectToAction("Index");


    }


    [HttpPost("ProfileImage/{id}")]
    public IActionResult ProfileImage([FromForm] IFormFile file,int id)
    {
       // int? id = HttpContext.Session.GetInt32("userId");
        string StringFileName = UploadFile(file);

        User editUser = _context.Users.First(e => e.UserId == id);
        editUser.Myimage = StringFileName;
        _context.SaveChanges();
        return Ok(editUser);


    }
    [HttpPost("Images/{img}")]
    public IActionResult Images(string img)
    {
        // int? id = HttpContext.Session.GetInt32("userId");
        var provider = new PhysicalFileProvider(WebHostEnvironment.WebRootPath);
        var contents = provider.GetDirectoryContents("Images");
        var objFiles = contents.FirstOrDefault(m => m.Name==img);

        return new JsonResult(objFiles);


       


    }

    //    public IActionResult Like(int id, int id2)
    // {

    //         Like mylike = new Like()
    //         {
    //             UserId = id,
    //             PostId = id2
    //         };
    //         _context.Add(mylike);
    //         _context.SaveChanges();
    //         return RedirectToAction("Index");
    // }
    //[HttpGet("GetAllUsers/{id}")]
    //public IActionResult GetAllUsers(int id)
    //{
    //    var AllUsers = _context.Users
    //       .Include(e => e.RequestsReciver)
    //        .Include(e => e.RequestsSender)
    //        .Where(e => e.UserId != id)
    //        .Where(e =>
    //                     (e.RequestsSender.Any(f => f.ReciverId == id) == false)
    //                    && (e.RequestsReciver.Any(f => f.SenderId == id) == false)
    //        ).Take(3)
    //        .ToList();
    //    return Ok(AllUsers);
    //}
    //[HttpGet("GetAllfriends/{id}")]
    //public IActionResult GetAllfriends(int id)
    //{
    //    var friends =_context.Requests.Include(e => e.Reciver).Include(e => e.Sender)
    //                        .Where(e => (e.ReciverId == id) || (e.SenderId == id))
    //                        .Where(e => e.Accepted == true)
    //                        .ToList();
    //    return Ok(friends);
    //}

    [HttpGet("User/{id}")]
    public IActionResult GetUSer(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.UserId == id);

        return Ok(user);

    }

    [HttpGet("GetAllUsers/{id}")]
    public IActionResult GetAllUsers(int id)
    {
        var AllUsers = _context.Users
           .Include(e => e.RequestsReciver)
            .Include(e => e.RequestsSender)
            .Where(e => e.UserId != id)
            .Where(e =>
                         (e.RequestsSender.Any(f => f.ReciverId == id) == false)
                        && (e.RequestsReciver.Any(f => f.SenderId == id) == false)
            ).Take(3)
            .ToList();
        var mapped = _mapper.Map<List<UserDTO>>(AllUsers);

        return Ok(mapped);
    }

    [HttpGet("GetAllfriends/{id}")]
    public IActionResult GetAllfriends(int id)
    {
        var friends = _context.Requests.Include(e => e.Reciver).Include(e => e.Sender)
                            .Where(e => (e.ReciverId == id) || (e.SenderId == id))
                            .Where(e => e.Accepted == true)
                            .ToList();
        var mapped = _mapper.Map<List<RequestDTO>>(friends);
        return Ok(mapped);
    }

    [HttpGet("GetAllPosts/{id}")]
    public IActionResult GetAllPosts(int id)
    {

        //var config = new MapperConfiguration(cfg => cfg.CreateMap<Post, PostDTO>());
        //var mapper = config.CreateMapper();

        var posts= _context.Posts.Include(e => e.Creator).Include(e => e.Likes).Include(e => e.Comments)
                                        .ThenInclude(e => e.UseriQekomenton).ThenInclude(e => e.RequestsReciver)
                                        .OrderByDescending(e => e.CreatedAt)

                                        .Where(e => (e.Creator.RequestsSender.Where(e => e.Accepted == true).Any(e => e.ReciverId == id) == false)
                                        || (e.Creator.RequestsReciver.Where(e => e.Accepted == true).Any(e => e.SenderId == id) == false)
                                        || e.Creator.UserId == id)
                                        .ToList();
        var mapped= _mapper.Map<List<PostDTO>>(posts);
        return Ok(mapped);
    }
    [HttpGet("GetAllRequests/{id}")]
    public IActionResult GetAllRequests(int id)
    {
        var requests = ViewBag.request = _context.Requests.Include(e => e.Reciver).Include(e => e.Sender)
                            .Where(e => e.ReciverId == id)
                            .Where(e => e.Accepted == false)
                            .ToList();
        var mapped = _mapper.Map<List<RequestDTO>>(requests);
        return Ok(mapped);
    }
        [HttpGet("SearchUsers")]
    public IActionResult SearchUsers(string searchString)
   {
        if(searchString == "" || searchString == null)
        {
            var Users = _context.Users.ToList();
            return Ok(Users);
        }

        var searchUsers = _context.Users.Where(s => s.FirstName!.ToLower().Contains(searchString.ToLower())).ToList();
        return Ok(searchUsers);
    }
    [HttpGet("GetUser/{id}")]
    public IActionResult GetUser(int id)
    {
        var user = _context.Users.FirstOrDefault(e=>e.UserId == id);
        var mapped = _mapper.Map<UserDTO>(user);
        return Ok(user);
    }

    [HttpPost("SendMessage")]
    public IActionResult SendMessage([FromBody] Message message)
    {
        if (message == null)
            return BadRequest();

        _context.Messages.Add(message);
        _context.SaveChanges();
       var Dbmessage =_context.Messages.Include(e => e.MessagesReciver).Include(e => e.MessagesSender).FirstOrDefault(e=>e.MessageId== message.MessageId);
        var mapped = _mapper.Map<MessageDTO>(Dbmessage);
        return Ok(mapped);
    }
    [HttpGet("GetMessagesSend/{MyId}/{FriendId}")]
    public IActionResult GetMessagesSend(int MyId , int FriendId)
    {
        var message = _context.Messages.Include(e=>e.MessagesReciver).Include(e=>e.MessagesSender).Where(e => (e.MessagesSenderId == MyId && e.MessagesReciverId == FriendId) ||( e.MessagesSenderId == FriendId && e.MessagesReciverId == MyId)).OrderBy(e=>e.Date).ToList();
        var mapped = _mapper.Map<List<MessageDTO>>(message);
        return Ok(mapped);
    }
    //[HttpGet("GetMessagesRecived/{MyId}/{FriendId}")]
    //public IActionResult GetMessagesRecived(int MyId, int FriendId)
    //{
    //    var message = _context.Messages.Where(e => e.SenderId == FriendId && e.ReciverId == MyId).OrderBy(e => e.Date).ToList();
    //    return Ok(message);
    //}


    //[HttpPost("OpenChat/{MyId}/{FriendId}")]
    //public IActionResult OpenChat(int MyId, int FriendId)
    //{

    //   var chat= _context.ChatRooms.FirstOrDefault(e=>e.MessagesSenderId == MyId && e.MessagesReciverId == FriendId);
    //    if (chat == null)
    //    {
    //        ChatRoom newchat = new ChatRoom()
    //        {

    //        };

    //    }
    //    _context.SaveChanges();
    
    //    return Ok();
    //}




    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

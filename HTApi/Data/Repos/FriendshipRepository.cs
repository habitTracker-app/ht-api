using HTApi.DTOs;
using HTApi.Models.ActionModels;
using HTApi.Models.Exceptions;
using HTAPI.Data;
using HTAPI.Models;
using HTAPI.Models.Friendships;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace HTApi.Data.Repos
{
    public interface IFriendshipRepository
    {
        Task<FriendshipDTO> CreateFriendship(AddFriend body);
        AllUserFriendRequestsDTO GetAllFriendRequests(User user);
        Task<FriendshipDTO> AnswerRequest(AnswerFriendRequest body, User user);
        Task<FriendshipDTO> CancelRequest(CancelFriendRequest body, User user);
    }
    public class FriendshipRepository : IFriendshipRepository
    {
        public static AppDbContext _db;

        public FriendshipRepository(IServiceProvider sp)
        {
            _db = sp.GetRequiredService<AppDbContext>();
        }

        public async Task<FriendshipDTO> CreateFriendship(AddFriend body)
        {
            User? requester = _db.Users.FirstOrDefault(u => u.UUID == body.RequesterUuid);
            User? target = _db.Users.FirstOrDefault(u => u.UUID == body.TargetUuid);
            
            if (requester == null) { throw new BadRequestException("The friendship requester does not exist.", 404); }
            if (target == null) { throw new BadRequestException("The friendship target does not exist.", 404); }
            if (requester.Id == target.Id) { throw new BadRequestException("The target cannot be the same user as the requester", 403);  }

            if(!this._canSendRequest(requester, target)) { throw new BadRequestException("You cannot send a request to this user.", 403); }

            Friendship f = new Friendship
            {
                Requester = requester,
                Target = target,
            };
            this._initFriendship(f);

            try
            {
                await _db.Friendship.AddAsync(f);
                await _db.SaveChangesAsync();

                return new FriendshipDTO(f);
            } catch (Exception ex)
            {
                throw new Exception("Error creating friendship on database.");
            }
        }

        public AllUserFriendRequestsDTO GetAllFriendRequests(User user) {
            List<Friendship> friendshipDTOs = [.. _db.Friendship
                                                    .Include(f => f.Requester)
                                                    .Include(f => f.Target)
                                                    .Include(f => f.Status)
                                                    .Where(f => f.TargetId == user.Id)];

            List<Friendship> fTarget = [.. friendshipDTOs.Where(f => f.TargetId == user.Id)];
            List<Friendship> rTarget = [.. friendshipDTOs.Where(f => f.RequesterId == user.Id)];

            AllUserFriendRequestsDTO obj = new AllUserFriendRequestsDTO(rTarget, fTarget);

            return obj;
        }

        public async Task<FriendshipDTO> AnswerRequest(AnswerFriendRequest body, User user)
        {
            Friendship? f = this._getFriendship(body.RequestId) ?? throw new BadRequestException("This friendship request does not exist.", 404);


            if (f.Status.Status != "pending")
            {
                throw new BadRequestException("This friend request is no longer accepting replies.", 409);
            }
            if (user.Id != f.TargetId)
            {
                throw new BadRequestException("You cannot accept this request because you are not the target.", 403);
            }

            if (body.Accept)
            {
                this._acceptFriendship(f);
            }
            else
            {
                this._denyFriendship(f);
            }
            try
            {
                _db.Friendship.Update(f);
                await _db.SaveChangesAsync();
                return new FriendshipDTO(f);
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }

        }
        
        public async Task<FriendshipDTO> CancelRequest(CancelFriendRequest body, User user)
        {
            Friendship f = this._getFriendship(body.RequestId) as Friendship;
            if(f == null)
            {
                throw new BadRequestException("This request does not exist.", 404);
            }
            if(f.Status.Status != "pending")
            {
                throw new BadRequestException("This request can no longer be cancelled.", 403);
            }

            if(f.Requester.Id != user.Id)
            {
                throw new BadRequestException("You cannot cancel this request because you did not send it.", 403);
            }
            _cancelFriendship(f);

            _db.Friendship.Update(f);
            await _db.SaveChangesAsync();

            return new FriendshipDTO(f);
        }

        private bool _canSendRequest(User requester, User target)
        {
            List<Friendship> all = [.. _db.Friendship
                                        .Include(f => f.Requester)
                                        .Include(f => f.Status)
                                        .Where(f => f.Requester.Id == requester.Id)
                                        .Where(f => f.Target.Id == target.Id)
                                        .Where(f => (f.Status.Status == "pending" || f.Status.Status == "approved") || (f.Status.Status == "denied" && (DateTime.UtcNow - f.UpdatedAt).Days < 7)) ];

            if(all.Count > 0) { return false; }

            return true;
        }
        private Friendship? _getFriendship(string id)
        {
            return _db.Friendship
                    .Include(f => f.Requester)
                    .Include(f => f.Target)
                    .Include(f => f.Status)
                    .FirstOrDefault(fr => fr.Id.ToString() == id);
        }
        private void _initFriendship(Friendship f)
        {
            f.Status = _db.FriendshipStatus.First(s => s.Status == "pending");
            f.CreatedAt = DateTime.UtcNow;
            f.UpdatedAt = DateTime.UtcNow;
        }
        private void _denyFriendship(Friendship f)
        {
            f.Status = _db.FriendshipStatus.First(s => s.Status == "denied");
            f.UpdatedAt = DateTime.UtcNow;
        }
        private void _acceptFriendship(Friendship f)
        {
            f.Status = _db.FriendshipStatus.First(s => s.Status == "approved");
            f.UpdatedAt = DateTime.UtcNow;
        }
        private void _cancelFriendship(Friendship f)
        {
            f.Status = _db.FriendshipStatus.First(s => s.Status == "cancelled");
            f.UpdatedAt = DateTime.UtcNow;
        }
    }
}

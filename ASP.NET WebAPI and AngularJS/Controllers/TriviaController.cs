using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Description;
using GeekQuiz.Models;

namespace ASP.NET_WebAPI_and_AngularJS.Controllers
{
    [Authorize]
    public class TriviaController : ApiController
    {
        private readonly TriviaContext _db = new TriviaContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            base.Dispose(disposing);
        }

        private async Task<TriviaQuestion> NextQuestionAsync(string userId)
        {
            var lastQuestionId = await _db.TriviaAnswers
                .Where(a => a.UserId == userId)
                .GroupBy(a => a.QuestionId)
                .Select(g => new { QuestionId = g.Key, Count = g.Count() })
                .OrderByDescending(q => new {q.QuestionId, q.Count})
                .Select(q => q.QuestionId)
                .FirstOrDefaultAsync();

            var questionsCount = await _db.TriviaQuestions.CountAsync();

            var nextQuestionId = (lastQuestionId % questionsCount) + 1;
            return await _db.TriviaQuestions.FindAsync(CancellationToken.None, nextQuestionId);
        }

        // GET api/Trivia
        [ResponseType(typeof(TriviaQuestion))]
        public async Task<IHttpActionResult> Get()
        {
            var userId = User.Identity.Name;

            TriviaQuestion nextQuestion = await NextQuestionAsync(userId);

            if (nextQuestion == null)
            {
                return NotFound();
            }

            return Ok(nextQuestion);
        }

        private async Task<bool> StoreAsync(TriviaAnswer answer)
        {
            _db.TriviaAnswers.Add(answer);

            await _db.SaveChangesAsync();
            var selectedOption = await _db.TriviaOptions.FirstOrDefaultAsync(o => o.Id == answer.OptionId
                && o.QuestionId == answer.QuestionId);

            return selectedOption.IsCorrect;
        }

        // POST api/Trivia
        [ResponseType(typeof(TriviaAnswer))]
        public async Task<IHttpActionResult> Post(TriviaAnswer answer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            answer.UserId = User.Identity.Name;

            var isCorrect = await StoreAsync(answer);
            return Ok(isCorrect);
        }
    }
}

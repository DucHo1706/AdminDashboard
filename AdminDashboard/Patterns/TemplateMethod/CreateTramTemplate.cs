using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Patterns.TemplateMethod
{
    public class CreateTramTemplate : CreateEntityTemplate<Tram>
    {
        public CreateTramTemplate(Db27524Context context) : base(context)
        {
        }

        protected override void PrepareModelState(Controller controller)
        {
            controller.ModelState.Remove("IdTram");
        }

        protected override Task<bool> ValidateAsync(Controller controller, Tram model)
        {
            return Task.FromResult(controller.ModelState.IsValid);
        }

        protected override async Task GenerateIdAsync(Tram model)
        {
            string newId;
            do
            {
                newId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            }
            while (await _context.Tram.AnyAsync(t => t.IdTram == newId));

            model.IdTram = newId;
        }
    }
}
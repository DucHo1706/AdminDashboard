using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;

namespace AdminDashboard.Patterns.TemplateMethod
{
    public abstract class CreateEntityTemplate<T>
    {
        protected readonly Db27524Context _context;

        protected CreateEntityTemplate(Db27524Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> ExecuteAsync(Controller controller, T model)
        {
            PrepareModelState(controller);
            await LoadViewDataAsync(controller, model);

            if (!await ValidateAsync(controller, model))
            {
                await LoadViewDataAsync(controller, model);
                return controller.View(model);
            }

            await GenerateIdAsync(model);
            await SaveAsync(model);

            return controller.RedirectToAction("Index");
        }

        protected virtual void PrepareModelState(Controller controller)
        {
        }

        protected virtual Task LoadViewDataAsync(Controller controller, T model)
        {
            return Task.CompletedTask;
        }

        protected abstract Task<bool> ValidateAsync(Controller controller, T model);

        protected virtual Task GenerateIdAsync(T model)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task SaveAsync(T model)
        {
            _context.Add(model);
            await _context.SaveChangesAsync();
        }
    }
}
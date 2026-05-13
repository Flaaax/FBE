namespace FBE.Scripts.Utils;

public static class PatchHelper
{
    public static async Task Wrap(Task originalTask, Func<Task> nextAsync)
    {
        await originalTask;
        await nextAsync();
    }

    public static async Task WrapAsync(Func<Task> task)
    {
        await task();
    }
}
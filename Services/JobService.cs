namespace TaskManagerAPI.Services
{
    public interface IJobService
    {
        void FireandForgetJob();
        void ReccuringJob();
        void DelayedJob();
        void ContinuedJob();
    }
    public class JobService : IJobService
    {

    }
}

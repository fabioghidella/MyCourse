namespace MyCourse.Models.Exceptions.Application;

public class InvalidVoteException : Exception
{
    public InvalidVoteException(int vote) : base($"The vote {vote} is not valid")
    {
    }
}

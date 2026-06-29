using System;

namespace TennisStats.Api.Exceptions
{
    public class PlayerNotFoundException : Exception
    {
        public PlayerNotFoundException(int id) 
            : base($"Player with id {id} was not found.")
        {
        }
    }
}

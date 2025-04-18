using System.Runtime.CompilerServices;

namespace Util;

public readonly struct Result<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;
    private readonly bool _ok;

    
    private Result(TError error)
    {
        _value = default;
        _ok = false;
        _error = error;
    }

    private Result(TValue value)
    {
        _value = value;
        _ok = true;
        _error = default;
    }

    public static Result<TValue, TError> Ok(TValue value)
    {
        return new Result<TValue, TError>(value);
    }

    public static Result<TValue, TError> Err(TError err)
    {
        return new Result<TValue, TError>(err);
    }

    public void UnwrapOr(Action<TValue> valueHandler, Action<TError> errorHandler)
    {
        if (_error is not null)
        {
            errorHandler(_error);
        }
        else if (_value is not null)
        {
            valueHandler(_value);
        }
        else
        {
            throw new ArgumentException("Broken result. Neither value nor error is present.");
        }
    }

    public static implicit operator Result<TValue, TError>(TError error)
    {
        return new(error);
    }

    public static implicit operator Result<TValue, TError>(TValue value)
    {
        return new(value);
    }
}
namespace ShitDB.Util;

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

    // only used as copy constructor
    private Result(TError? error, TValue? value, bool ok)
    {
        _value = value;
        _ok = ok;
        _error = error;
    }

    public static Result<TValue, TError> Ok(TValue value)
    {
        return new Result<TValue, TError>(value);
    }

    public static Result<TValue, TError> Err(TError err)
    {
        return new Result<TValue, TError>(err);
    }

    public void HandleOk(Action<TValue> valueHandler)
    {
        if (_value is not null && _ok) valueHandler(_value);
    }

    public void HandleErr(Action<TError> errorHandler)
    {
        if (_error is not null && !_ok) errorHandler(_error);
    }

    public void UnwrapOr(Action<TValue> valueHandler, Action<TError> errorHandler)
    {
        if (_error is not null && !_ok)
            errorHandler(_error);
        else if (_value is not null && _ok)
            valueHandler(_value);
        else
            throw new ArgumentException("Broken result. Neither value nor error is present.");
    }

    public TValue Unwrap()
    {
        if (_error is not null && !_ok) throw new Exception(_error.ToString());

        if (_value is not null && _ok) return _value;

        throw new ArgumentException("Broken result. Neither value nor error is present.");
    }

    public Result<TNewTValue, TNewTError> Map<TNewTValue, TNewTError>(Func<TValue, TNewTValue?> mapOk,
        Func<TError, TNewTError?> mapErr)
    {
        return new Result<TNewTValue, TNewTError>(_error is not null && !_ok ? mapErr(_error) : default,
            _value is not null && _ok ? mapOk(_value) : default, _ok);
    }

    public Result<TNewTValue, TError> MapOk<TNewTValue>(Func<TValue, TNewTValue> map)
    {
        return new Result<TNewTValue, TError>(_error, _value is not null && _ok ? map(_value) : default, _ok);
    }

    public Result<TValue, TNewTError> MapErr<TNewTError>(Func<TError, TNewTError> map)
    {
        return new Result<TValue, TNewTError>(_error is not null && !_ok ? map(_error) : default, _value, _ok);
    }

    public TError UnwrapErr()
    {
        if (_error is not null && !_ok) return _error;

        if (_value is not null && _ok) throw new Exception("UnwrapErr called on valid result.");

        throw new ArgumentException("Broken result. Neither value nor error is present.");
    }

    public bool IsOk()
    {
        return _ok;
    }

    public bool IsErr()
    {
        return !_ok;
    }

    public static implicit operator Result<TValue, TError>(TError error)
    {
        return new Result<TValue, TError>(error);
    }

    public static implicit operator Result<TValue, TError>(TValue value)
    {
        return new Result<TValue, TError>(value);
    }
}
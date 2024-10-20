from typing import Dict, Callable, Tuple, Any
import datetime
import dateutil
import time
from functools import wraps
import tiktoken
import re
import gc
import sys
import os

if __name__ == '__main__': 
    sys.path.append(os.path.dirname(os.path.abspath(__file__ + './')))
    sys.path.append(os.path.dirname(os.path.abspath(__file__ + '../../')))
from shared.OpenTelemetry import log_debug, log_exception, log_info, log_error, log_warning

DebugMode = os.environ.get("COPILOT_DEBUG", "").lower() == "true"

def get_search_filter(filters: list[tuple[str,...]]):
    def rep(x):
        if isinstance(x, str):
            if "'" in x:
                log_warning(f'Filter value "{x}" contains single quotes') # TODO: remove after some log splunking
                x = x.replace("'", "''") # escape single quotes by doubling
            r = f"'{x}'" # same as repr(r)
            return r
        elif isinstance(x, datetime.datetime):
            return get_index_timestr(x)
        return x
    filter = " and ".join( [
                f"({f[0]} {'eq' if len(f)==2 else f[2]} {rep(f[1])})"
            for f in filters ])
    return filter

def recurse_remove_empty_elements(d):
    """recursively remove empty lists, empty dicts, or None elements from a dictionary"""

    def empty(x):
        return x is None # or x == {} or x == []

    if not isinstance(d, (dict, list)):
        return d
    elif isinstance(d, list):
        return [v for v in (recurse_remove_empty_elements(v) for v in d) if not empty(v)]
    else:
        return {k: v for k, v in ((k, recurse_remove_empty_elements(v)) for k, v in d.items()) if not empty(v)}

def elapsed_ms(fn:Callable, *args, **kwargs) -> Tuple[int, Any]:
    """_summary_
        Measure elapsed time for a function call and return (elapsed_ms, return_value)
        Does not handle exceptions
    """
    start = time.time()
    ret = fn(*args, **kwargs)
    end = time.time()
    ms = int((end - start) * 1000)
    return ms, ret

# Wrap function call using this as function decorator to log timing, exceptions and metrics
def timed(
        msg = None, 
        enter_msg = True,
        count_all_metric_fn = None, duration_all_metric_fn = None,
        count_ok_metric_fn = None, duration_ok_metric_fn = None,
        count_fail_metric_fn = None, duration_fail_metric_fn = None
    ):
    """This decorator prints the execution time for the decorated function.
    Fn decoration:  @timed("thing done", enter_msg="about to do the thing")
    msg: optional additional msg to be added to exit timing message
    enter_msg: optional (def=True) Set to True to also print msg on entry, or str to add custom enter msg
    """

    def wrap_outer(func):

        @wraps(func)
        def wrap_inner(*args, **kwargs):
            if enter_msg:
                emsg = f": {enter_msg}" if enter_msg != True else ""
                log_info(f"Entering {func.__name__} {emsg}")

            if count_all_metric_fn: count_all_metric_fn()
            start = time.time()

            try:
                result = func(*args, **kwargs)
                end = time.time()
                end_ms = int((end - start) * 1000)
                if count_ok_metric_fn: count_ok_metric_fn()
                if duration_ok_metric_fn: duration_ok_metric_fn(end_ms)
                if duration_all_metric_fn: duration_all_metric_fn(end_ms, True)
                if count_all_metric_fn: count_all_metric_fn(True)
            except Exception as e:
                end = time.time()
                end_ms = int((end - start) * 1000)
                if count_fail_metric_fn: count_fail_metric_fn()
                if duration_fail_metric_fn: duration_fail_metric_fn(end_ms)
                if duration_all_metric_fn: duration_all_metric_fn(end_ms, False)
                if count_all_metric_fn: count_all_metric_fn(False)
                diff = round(end - start, 2)
                log_warning(f"{func.__name__}: call failed after {diff}s", exc_info = e)
                raise

            message = f" : {msg}" if msg else ""
            log_info("{} ran in {}s {}".format( 
                        func.__name__, 
                        round(end - start, 2),
                        message))
            return result

        return wrap_inner

    return wrap_outer

def path_join(*args):
    """os.path.join and Pathlib both do things we don't want"""
    return '/'.join([p.strip('/') for p in args])

token_encoding = tiktoken.get_encoding("cl100k_base")

def get_num_tokens(text: str) -> int:
    tokens = token_encoding.encode(text)
    return len(tokens)

def pluck(dict, *args): 
    return (dict.get(arg, -1) for arg in args)
    
def default(val, default):
    return val if val is not None else default 

def file_looks_like(file:str, extension: str) -> bool:
    """ Check if a file has the extension specificed.
    Handle the 'file.pdf$123123' format that was used for period of time - 
     extension now is appended to the end"""
    file, extension = file.lower(), extension.lower()
    regex = f"^.+\\.{extension}(\\$\\d+)?$"
    return re.match(regex, file) is not None

def get_index_timestr(time: datetime.datetime):
    #'2024-03-14T14:37:22.6930Z' - Azure search docs say will be rounded up, but must strip microseconds or will be rejected, and zulu must be added
    # return f"{time.strftime('%Y-%m-%dT%X.%f')[:-2]}Z"
    # This behavior appears to have been fixed in Azure Search - now accepts microseconds
    return time.isoformat()

def try_parse_isodate(date_str: str)  -> datetime.datetime | None:
    try:
        dt = dateutil.parser.isoparse(date_str)
        if dt.tzinfo is None or dt.tzinfo != dateutil.tz.UTC:
            # We should always have UTC dates - if not, force UTC so that
            #  time comparisons don't raise exceptions, but comparisions will be off
            log_warning(f"Time {date_str} is not in UTC")
            dt = dt.replace(tzinfo=datetime.timezone.utc)
        return dt
    except:
        log_error(f"Failed to parse isoformat time: {date_str}")
        return None

def fix_request_date(d:Dict, key:str) -> None:
    if key not in d: return
    date_str = d[key]
    if date_str.endswith("Z"):
        d[key] = date_str.replace("Z", "+00:00")

def fix_request_date_timestamp(d:Dict, key:str) -> None:
    v = d.get(key)
    if not v: return
    if isinstance(v, int) or isinstance(v, float): return
    dt = try_parse_isodate(v)
    if dt: 
        d[key] = dt.timestamp()
        return
    raise ValueError(f"Invalid date format for '{key}': {v}")

#@timed()
def gc_collect() -> None:
    """Force a garbage collection run"""
    n_collected = gc.collect()
    log_debug(f"GC: {n_collected} objects collected")

if __name__ == '__main__':
    if False:
        assert True == file_looks_like("foo.pdf", "pdf")
        assert True == file_looks_like("foo.pDf", "pdf")
        assert True == file_looks_like("foo.pdf$123", "pdf")
        assert False == file_looks_like("foo$123", "pdf")
        assert False == file_looks_like("foo.txt", "pdf")
        assert False == file_looks_like("foo.pdfx", "pdf")
    if False:
        @timed()
        def foo():
            print("foo")
            print(1/0)
        foo()
    if True:
        print( try_parse_isodate("2024-04-05T19:46:53"))
        print( try_parse_isodate("2024-04-05T19:46:53.57"))
        print( try_parse_isodate("2024-04-05T19:46:53.57Z"))
        print( try_parse_isodate("2024-04-05T19:46:53.57+00:00"))
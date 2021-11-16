using namespace System.Management.Automation
using namespace System.Collections
#attempt at providing a class to implement functional effects in Powershell.
#inspired by Scala ZIO library   


Class IO {
    #description of what we want to do
    [ScriptBlock]$effect
    $args

    hidden [InitialSessionState]$state = [System.Management.Automation.Runspaces.InitialSessionState]::CreateDefault()
    hidden [Host.PSHost]$_host = $Host
    IO ([ScriptBlock]$func) {
        $this.effect = $func
    }
    IO ([ScriptBlock]$func, $_args) {
        $this.effect = $func
        $this.args = $_args
    }
    IO () { }

    #return pure value, may be effectfully obtained so InvokeRun is not implcitly called.
    #we use this method to pass input to a following IO in the chain, which would then
    #in its own context call InvokeRun
    [IO] RawVal ([Object]$o) {
        return [IO]::new( { Param($x); return $x }, $o)
    }
    #We may need to pass in variables from the outer scope to our
    #multithreaded environment.
    [Void] AddSessionVariable(
            [object]$variable,
            [String]$variable_name,
            [String]$variable_description
    ) {
        $this.state.Variables.Add([Runspaces.SessionStateVariableEntry]::new(
                $variable_name,
                $variable,
                $variable_description
        ))
    }
    #loop over the collection, parrallel style
    #this will need to be refined and potentially implement locking constructs as well.
    [void] ForEachParN([int]$n, [ArrayList]$xs) {

        #build a threadpoool of n size and pass it our global state
        #this will allow us to pass in variables from the AddSessionVariables method
        $runspace_pool = [RunspaceFactory]::CreateRunspacePool(1, $ENV:NUMBER_OF_PROCESSORS, $this.state, $this._host);
        $runspace_pool.Open()
        $external_parameters = $this.effect.ast.paramBlock.parameters
        #arr to hold our [Powershell] objects
        [hashtable[]]$instances = @()

        #generate a thread for each iteration of the loop,
        #dependant on number of requested threads in the pool
        #we will use the the script block of $this instance of IO
        #we then match on the provided parameters for our scriptblock
        for ($i = 0; $i -lt $xs.Count; $i++) {
            $ps_instance = [Powershell]::Create()
            $ps_instance.RunspacePool = $runspace_pool
            $ps_instance.AddScript($this.effect)
            for ($j = 0; $j -lt $external_parameters.Count; $j++) {
                $var = $external_parameters[$j].name.variablePath.UserPath
                $ps_instance.AddParameter($var, $xs[$i]["$($var)"])
            }
            $inst = [hashtable]@{
                "instance" = $ps_instance;
                "task" = $ps_instance.BeginInvoke()
            }
            $instances += ($inst)
        }
        #block until mt task is complete
        while ($instances.task.IsCompleted -contains $false) {

        }
        #Prevent a memory leak; always dispose.
        $instances.ForEach({
            $_.instance.Dispose();
        })
        $runspace_pool.Dispose();
    }

    [Void]
    DoUntil([int]$duration, [switch]$forever)
    {
        if ($forever)
        {
            while ($true)
            {
                $this.InvokeRun()
            }
        }
        else
        {
            $fut = [DateTime]::Now.AddSeconds($duration)
            while ([DateTime]::Now.Second -ne $fut.Second)
            {
                $this.InvokeRun()
            }
        }
    }
    [Void]
    SetInterval([double]$interval)
    {
        while ($true)
        {
            $this.InvokeRun()
            [System.Threading.Thread]::Sleep($interval * 1000)
        }
    }

    #Produce a new [IO]C with the result of [IO]A as the args to the effect of [IO]B
    [IO]
    FlatMap([IO]$B)
    {
        return [IO]::new($B.effect,$this.InvokeRun())
    }
    #alias for above function, this is not a true definition of FlatMapping, but it keeps with ZIO parlance
    [IO]
    to([IO]$B)
    {
        return [IO]::new($B.effect,$this.InvokeRun())
    }

    # function composition f(g(x))
    [IO]
    Compose([IO]$B)
    {
        return [IO]::new($this.effect,$B.effect.InvokeReturnAsIs($this.args))
    }

    #run this IO and then the next IO
    [Void]
    AndThen([IO]$B)
    {
        $this.InvokeRun();
        $B.InvokeRun()
    }

    #actually run the effect and return its output
    [Object]
    InvokeRun()
    {
        if ($null -ne $this.args)
        {
            return $this.effect.InvokeReturnAsIs($this.args);
        }
        else
        {
            return $this.effect.InvokeReturnAsIs()
        }
    }
}

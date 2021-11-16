class SimpleThread : System.IDisposable {
    [String]$id
    hidden [powershell]$thread_instance = [powershell]::Create()
    [object]$local_data

    SimpleThread(
            [powershell]$underlying,
            [scriptblock]$task,
            [object]$local_data
    ){
        $this.id = $underlying.InstanceId.Guid.ToString(),
        #kill temp thread ojbect, update ref to pass PS 
        $this.thread_instance.Dispose()
        $this.thread_instance = $underlying
        $this.thread_instance.AddScript($task)
        $this.local_data = $local_data
    }

    [Object]
    ToAsync(){
        if($null -ne $this.local_data) {
            $this.thread_instance.AddParameter(
                $this.thread_instance.commands.commands[0].parameters.name,
                $this.local_data
                )
            return $this.thread_instance.BeginInvoke()
        }
        else {
            return $this.thread_instance.BeginInvoke()
        }

    }

    [Void]
    Dispose() {
        $this.thread_instance.Dispose()
        $this.thread_instance = $null
    }
}

class ThreadPool : System.IDisposable {

    [System.Collections.Concurrent.ConcurrentQueue[System.Object]]$workers = [System.Collections.Concurrent.ConcurrentQueue[System.Object]]::new()
    [System.Collections.Concurrent.ConcurrentQueue[System.Object]]$jobs = [System.Collections.Concurrent.ConcurrentQueue[System.Object]]::new()

    ThreadPool() {}

    [Boolean]
    Add(
        [SimpleThread]$t
    ) {
        try {
            return $this.workers.TryAdd($t)
        }
        catch {
            return $false
        }
    }

    
    [Void]
    RunAll()
    {
        $this.workers.ForEach({
            $this.jobs.TryAdd($_.ToAsync())
        })
        $this.jobs.ToArray().ForEach({
            while($_.IsCompleted -eq $false) {
                #poll spin each async job until complete, the outer foreach is synchronous and the evaluation is immediate
                #ie trivial jobs will have executed too quickly to ever hit this sloop 
            }
        })
    }

    [Void]
    Dispose(){
        $this.workers.foreach({
            $_.Dispose()
        })
    }
    
}

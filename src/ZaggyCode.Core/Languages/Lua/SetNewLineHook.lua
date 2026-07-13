local function onLineExec(event, line_number)
   local info = debug.getinfo(2)

   if info and info.source and not info.source:match("^@") then
      __clr_DebugLineUpdated_raise(line_number)
      __clr_wait()
   end
end

debug.sethook(onLineExec, "l")
__debug("DebugLineUpdated event have been subscribed")
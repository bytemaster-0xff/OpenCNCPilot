using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Signal
    {
        public List<ContactRef> Contacts { get; private set; }
        public List<Wire> Wires { get; private set; }

        public string Name { get; set; }

        public static Signal Create(XElement element)
        {
            return new Signal()
            {
                Name = element.GetString("name"),
                Wires = (from childWires in element.Descendants("wire") select Wire.Create(childWires)).ToList(),
                Contacts = (from refs in element.Descendants("contactref") select ContactRef.Create(refs)).ToList(),
            };
        }

        public List<Wire> UnroutedWires
        {
            get { return Wires.Where(wire => wire.Layer == 19).ToList(); }
        }

        public List<Wire> TopWires
        {
            get { return Wires.Where(wire => wire.Layer == 1).ToList(); }
        }

        public List<Wire> BottomWires
        {
            get { return Wires.Where(wire => wire.Layer == 16).ToList(); }
        }

        public List<Trace> TopTraces
        {
            get
            {
                var unprocessedWires = TopWires.ToList();
                var traces = new List<Trace>();
                var completed = false;

                while (!completed)
                {
                    /* Grab first fire of list of wires not processed */
                    var candidateWire = unprocessedWires.FirstOrDefault();
                    if (candidateWire != null)
                    {
                        /* This wire is process so remove it */
                        unprocessedWires.Remove(candidateWire);

                        /* Create our new trace, add the first wire and add it to the traces */
                        var trace = new Trace();
                        trace.Wires.Add(candidateWire);
                        traces.Add(trace);

                        /* Continue searching if we have a candidate coming in OR we have a new candidate that was put on the trace to review */
                        while (candidateWire != null)
                        {
                            var newCandidates = new List<Wire>();

                            var wiresProcessed = new List<Wire>();

                            foreach (var wire in unprocessedWires)
                            {
                                if (candidateWire.Rect.X2 == wire.Rect.X1 && candidateWire.Rect.Y2 == wire.Rect.Y1)
                                {
                                    /* Add a new candidate to review */
                                    newCandidates.Add(wire);

                                    /* Make the junctions to the connected traces/wires */
                                    candidateWire.EndJunctions.Add(wire);
                                    wire.StartJunctions.Add(candidateWire);

                                    /* Of course we add it to the trace */
                                    trace.Wires.Add(wire);
                                    wiresProcessed.Add(wire);
                                }
                                else if (candidateWire.Rect.X1 == wire.Rect.X2 && candidateWire.Rect.Y1 == wire.Rect.Y2)
                                {
                                    /* Add a new candidate to review */
                                    newCandidates.Add(wire);

                                    /* Make the junctions to the connected traces/wires */
                                    candidateWire.StartJunctions.Add(wire);
                                    wire.EndJunctions.Add(candidateWire);

                                    /* Of course we add it to the trace */
                                    trace.Wires.Add(wire);
                                    wiresProcessed.Add(wire);
                                }
                            }

                            /* If the wire was added in this pass, remove it from the unprocessedWires list */
                            foreach (var wire in wiresProcessed)
                            {
                                unprocessedWires.Remove(wire);
                            }

                            /* If we have a new candidate review it */
                            candidateWire = newCandidates.FirstOrDefault();
                            if(candidateWire != null)
                            {
                                newCandidates.Remove(candidateWire);
                            }
                            
                        }
                    }
                    else
                    {
                        return traces;
                    }


                    completed = !unprocessedWires.Any();
                }

                return traces;
            }
        }

        public List<Trace> BottomTraces
        {
            get
            {
                var traces = new List<Trace>();


                return traces;
            }
        }
    }
}

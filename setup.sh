#!/bin/bash
echo "🎉 ISKOWALK Backend Setup Starting..."
cat > server.js << 'ENDOFFILE'
const express = require('express');
const cors = require('cors');
const { createClient } = require('@supabase/supabase-js');
const app = express();
const PORT = process.env.PORT || 5000;
app.use(cors());
app.use(express.json());
const supabaseUrl = 'https://ugpmsogfrkachpdbgjvq.supabase.co';
const supabaseKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InVncG1zb2dmcmthY2hwZGJnanZxIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjgxMDcxMzEsImV4cCI6MjA4MzY4MzEzMX0.9S8qM0kGrIsonWR8Ev56ddYLljTUzb1-hdWmuPtK9ZE';
const supabase = createClient(supabaseUrl, supabaseKey);
app.get('/api/test', (req, res) => { res.json({ message: '✅ ISKOWALK Backend is running!' }); });
app.post('/api/walk-requests', async (req, res) => { const { userId, from, fromOrigin, to, dateOfWalk, timeOfWalk, additionalNotes, attire } = req.body; if (!userId || !from || !to || !dateOfWalk || !timeOfWalk) { return res.status(400).json({ error: 'Missing required fields' }); } const { data: userData } = await supabase.from('users').select('*').eq('id', userId).single(); if (!userData) { return res.status(404).json({ error: 'User not found' }); } const { data: newRequest } = await supabase.from('walk_requests').insert([{ requester_id: userId, requester_name: userData.name, from_location: from, from_origin: fromOrigin, to_location: to, date_of_walk: dateOfWalk, time_of_walk: timeOfWalk, additional_notes: additionalNotes, attire: attire, status: 'pending' }]).select().single(); res.status(201).json({ message: 'Walk request created', request: newRequest }); });
app.get('/api/walk-requests/available/:userId', async (req, res) => { const { data } = await supabase.from('walk_requests').select('*').eq('status', 'pending').neq('requester_id', req.params.userId).order('created_at', { ascending: false }); res.json({ requests: data || [] }); });
app.get('/api/walk-requests/active/:userId', async (req, res) => { const { data } = await supabase.from('walk_requests').select('*').eq('requester_id', req.params.userId).in('status', ['pending', 'accepted']).order('created_at', { ascending: false }); res.json({ requests: data || [] }); });
app.get('/api/walk-requests/history/:userId', async (req, res) => { const { data } = await supabase.from('walk_requests').select('*').or(`requester_id.eq.${req.params.userId},companion_id.eq.${req.params.userId}`).in('status', ['completed', 'cancelled']).order('updated_at', { ascending: false }); res.json({ requests: data || [] }); });
app.post('/api/walk-requests/:requestId/accept', async (req, res) => { const { userId } = req.body; const { data: userData } = await supabase.from('users').select('*').eq('id', userId).single(); const { data } = await supabase.from('walk_requests').update({ status: 'accepted', companion_id: userId, companion_name: userData.name }).eq('id', req.params.requestId).select().single(); res.json({ message: 'Request accepted', request: data }); });
app.delete('/api/walk-requests/:requestId', async (req, res) => { const { data } = await supabase.from('walk_requests').update({ status: 'cancelled' }).eq('id', req.params.requestId).select().single(); res.json({ message: 'Request cancelled', request: data }); });
app.listen(PORT, () => { console.log(`🚀 ISKOWALK API running on http://localhost:${PORT}`); });
ENDOFFILE
echo "✅ server.js created!"
if [ ! -d "node_modules" ]; then npm install express cors @supabase/supabase-js; fi
echo "✅ Setup complete! Run: node server.js"
